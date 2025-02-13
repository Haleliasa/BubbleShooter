#nullable enable

using Bubbles;
using Field;
using GoogleMobileAds.Api;
using Levels;
using System;
using UI;
using UI.Dialog;
using UI.Pages;
using UnityEngine;

public class Bootstrap : MonoBehaviour {
    [AddressablesLabel]
    [SerializeField]
    private string[] levelAddressablesLabels = null!;

    private readonly ILogger logger = Debug.unityLogger;
    private const string logTag = nameof(Bootstrap);

    private void Start() {
        Field.Field field = Get<Field.Field>();
        BubbleShooter bubbleShooter = Get<BubbleShooter>();
        MenuPage menuPage = Get<MenuPage>();
        GamePage gamePage = Get<GamePage>();
        Canvas canvas = Get<Canvas>();
        GameController gameController = Get<GameController>();
        UiController uiController = Get<UiController>();
        ObjectPool<FieldCell> fieldCellPool = Get<ObjectPool<FieldCell>>();
        ObjectPool<StaticBubble> staticBubblePool = Get<ObjectPool<StaticBubble>>();
        ObjectPool<ProjectileBubble> projectileBubblePool = Get<ObjectPool<ProjectileBubble>>();
        ObjectPool<Dialog> dialogPool = Get<ObjectPool<Dialog>>();
        ObjectPool<DialogButton> dialogButtonPool = Get<ObjectPool<DialogButton>>();

        PooledDialogService dialogService = new(dialogPool, dialogButtonPool, canvas.transform);

        field.Init(
            new FieldCellFactory(fieldCellPool),
            new StaticBubbleFactory(staticBubblePool)
        );

        menuPage.Init(dialogService);
        gamePage.Init(gameController, dialogService);

        uiController.Init(this.logger);

        gameController.Init(
            field,
            bubbleShooter,
            new AddressablesLevelLoader(this.levelAddressablesLabels),
            projectileBubblePool,
            uiController,
            dialogService
        );

        MobileAds.Initialize(status => this.logger.Log(logTag, "Ads initialized", this));

        static T Get<T>() where T : UnityEngine.Object {
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
