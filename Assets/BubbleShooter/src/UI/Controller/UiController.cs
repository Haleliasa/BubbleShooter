#nullable enable

using System;
using UnityEngine;

namespace BubbleShooter.UI {
    public class UiController : MonoBehaviour, IUiController {
        private UiPage[] pages = null!;

        private ILogger logger = null!;
        private const string logTag = nameof(UiController);

        private int currentPageIndex = -1;

        public void Init(ILogger logger) {
            this.pages = FindObjectsByType<UiPage>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            foreach (UiPage page in this.pages) {
                page.Register(this);
                page.Close();
            }

            this.logger = logger;
        }

        public void GoTo(string uiPage) {
            int newPageIndex = Array.FindIndex(this.pages, p => p.Name == uiPage);

            if (newPageIndex < 0) {
                this.logger.LogError(logTag, $"Page {uiPage} not found", this);

                return;
            }

            if (this.currentPageIndex >= 0) {
                this.pages[this.currentPageIndex].Close();
            }

            this.pages[newPageIndex].Open();
            this.currentPageIndex = newPageIndex;
        }
    }
}
