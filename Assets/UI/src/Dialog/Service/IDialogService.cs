using System.Collections.Generic;
using System.Threading.Tasks;

namespace UI.Dialog {
    public interface IDialogService {
        Dialog Open(
            string title,
            string body,
            IEnumerable<string> options
        );

        void Open(
            Dialog dialog,
            string title,
            string body,
            IEnumerable<string> options
        );

        DialogResult<string> OpenAsync(
            string title,
            string body,
            IEnumerable<string> options
        );

        Task<string> OpenAsync(
            Dialog dialog,
            string title,
            string body,
            IEnumerable<string> options
        );

        DialogResult<T> OpenAsync<T>(
            string title,
            string body,
            IEnumerable<DialogOption<T>> options
        );

        Task<T> OpenAsync<T>(
            Dialog dialog,
            string title,
            string body,
            IEnumerable<DialogOption<T>> options
        );
    }
}
