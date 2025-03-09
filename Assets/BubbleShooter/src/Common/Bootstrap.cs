#nullable enable

using BubbleShooter.Ads;
using BubbleShooter.Bubbles;
using BubbleShooter.Core;
using BubbleShooter.Field;
using BubbleShooter.Levels;
using BubbleShooter.UI;
using BubbleShooter.UI.Dialog;
using BubbleShooter.UI.Pages;
using GoogleMobileAds.Api;
using System;
using UnityEngine;

namespace BubbleShooter {
    public class Bootstrap : MonoBehaviour {
        [SerializeField]
        private GameConfig gameConfig = null!;

        [SerializeField]
        private AdsConfig adsConfig = null!;

        [AddressablesLabel]
        [SerializeField]
        private string[] levelAddressablesLabels = null!;

        private readonly ILogger logger = Debug.unityLogger;
        private const string logTag = nameof(Bootstrap);

        private void Start() {
            Field.Field field = Find<Field.Field>();
            ProjectileBubbleShooter bubbleShooter = Find<ProjectileBubbleShooter>();
            Canvas canvas = Find<Canvas>();
            MenuPage menuPage = Find<MenuPage>();
            GamePage gamePage = Find<GamePage>();
            GameController gameController = Find<GameController>();
            UiController uiController = Find<UiController>();
            ObjectPool<FieldCell> fieldCellPool = Find<ObjectPool<FieldCell>>();
            ObjectPool<StaticBubble> staticBubblePool = Find<ObjectPool<StaticBubble>>();
            ObjectPool<ProjectileBubble> projectileBubblePool = Find<ObjectPool<ProjectileBubble>>();
            ObjectPool<Dialog> dialogPool = Find<ObjectPool<Dialog>>();
            ObjectPool<DialogButton> dialogButtonPool = Find<ObjectPool<DialogButton>>();

            PooledDialogService dialogService = new(dialogPool, dialogButtonPool, canvas.transform);

            field.Init(
                new FieldCellFactory(fieldCellPool),
                new StaticBubbleFactory(staticBubblePool)
            );

            menuPage.Init(dialogService);
            gamePage.Init(gameController, dialogService);

            uiController.Init(this.logger);

            gameController.Init(
                this.gameConfig,
                this.adsConfig,
                field,
                bubbleShooter,
                new AddressablesLevelLoader(this.levelAddressablesLabels),
                projectileBubblePool,
                uiController,
                dialogService,
                this.logger
            );

            MobileAds.Initialize(status => this.logger.Log(logTag, "Ads initialized", this));

            uiController.GoTo(UiPages.menu);

            static T Find<T>() where T : UnityEngine.Object {
                return FindAnyObjectByType<T>(FindObjectsInactive.Include)
                    ?? throw new Exception($"{typeof(T).Name} not found");
            }
        }

        private class FieldCellFactory : IFieldCellFactory {
            private readonly ObjectPool<FieldCell> pool;

            public FieldCellFactory(ObjectPool<FieldCell> pool) {
                this.pool = pool;
            }

            public FieldCell Create() {
                return this.pool.GetConcrete();
            }

            public void Destroy(FieldCell cell) {
                this.pool.Return(cell);
            }
        }

        private class StaticBubbleFactory : IFieldObjectFactory {
            private readonly ObjectPool<StaticBubble> pool;

            public StaticBubbleFactory(ObjectPool<StaticBubble> pool) {
                this.pool = pool;
            }

            public IFieldObject Create(Color color) {
                IDisposableObject<StaticBubble> bubble = this.pool.Get();
                bubble.Object.Init(color, pooled: bubble);

                return bubble.Object;
            }
        }
    }
}
