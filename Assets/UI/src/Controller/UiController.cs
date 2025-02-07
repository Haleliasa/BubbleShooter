#nullable enable

using System;
using UnityEngine;

namespace UI {
    public class UiController : MonoBehaviour, IUiController {
        [SerializeField]
        private PageSetting[] pages = null!;

        [SerializeField]
        private string startPage = null!;

        private ILogger logger = null!;
        private const string logTag = nameof(UiController);

        private int currentPageIndex = -1;

        public void Init(ILogger logger) {
            for (int i = 0; i < this.pages.Length; i++) {
                UiPage page = this.pages[i].obj;
                page.Register(this);
                ClosePage(page);
            }

            this.logger = logger;

            this.GoTo(this.startPage);
        }

        public void GoTo(string uiPage) {
            int newPageIndex = Array.FindIndex(this.pages, p => p.name == uiPage);

            if (newPageIndex < 0) {
                this.logger.LogError(logTag, $"Page {uiPage} not found", this);

                return;
            }

            if (this.currentPageIndex >= 0) {
                ClosePage(this.pages[this.currentPageIndex].obj);
            }

            OpenPage(this.pages[newPageIndex].obj);
            this.currentPageIndex = newPageIndex;
        }

        private static void OpenPage(UiPage page) {
            page.gameObject.SetActive(true);
        }

        private static void ClosePage(UiPage page) {
            page.gameObject.SetActive(false);
        }

        [Serializable]
        private struct PageSetting {
            public string name;
            public UiPage obj;
        }
    }
}
