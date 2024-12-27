using System;
using System.Windows;
using NextDesign.Desktop;
using NextDesign.Extension;

namespace LoadUCScenario
{
    /// <summary>
    /// エクステンションのエントリポイントです
    /// </summary>
    public class LoadUCScenarioEntryPoint : IExtension
    {
        /// <summary>
        /// アプリケーション
        /// </summary>
        private IApplication App { get; set; }

        #region Activate/Deactivate

        /// <summary>
        /// エクステンションの初期化時の処理です。
        /// </summary>
        /// <param name="context">エクステンションのコンテキストです。</param>
        public void Activate(IContext context)
        {
            App = context.App;
        }

        /// <summary>
        /// エクステンションの終了前の処理です。
        /// </summary>
        /// <param name="context">エクステンションのコンテキストです。</param>
        public void Deactivate(IContext context)
        {
        }

        #endregion


        #region Commands

        /// <summary>
        /// Helloのコマンド実行です
        /// </summary>
        /// <param name="context"></param>
        /// <param name="p"></param>
        public void SayHello(ICommandContext context, ICommandParams p)
        {
            App.Window.UI.ShowMessageBox("Hello World!!", "LoadUCScenario");
        }

        #endregion

    }
}
