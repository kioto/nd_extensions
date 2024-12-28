using System;
using System.Windows;
using NextDesign.Desktop;
using NextDesign.Extension;
using NextDesign.Core;

namespace LoadUCScenario
{
    /// <summary>
    /// エクステンションのエントリポイントです
    /// </summary>
    public class LoadUCScenarioEntryPoint : IExtension
    {
        private IContext m_Context;

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
            m_Context = context;
        }

        /// <summary>
        /// エクステンションの終了前の処理です。
        /// </summary>
        /// <param name="context">エクステンションのコンテキストです。</param>
        public void Deactivate(IContext context)
        {
        }

        #endregion

        private IModel findPackage(IModel parent, string pathName)
        {
            IModel model = null;
            foreach (var package in parent.FindChildrenByClassDisplayName("パッケージ", recursive: true))
            {
                if (package.ModelPath == pathName)
                {
                    model = package;
                    break;
                }
            }

            return model;
        }

        #region Commands
        /// <summary>
        /// ユースケースシナリオ読み込みのコマンド実行です
        /// </summary>
        /// <param name="context"></param>
        /// <param name="p"></param>
        public void Load(ICommandContext context, ICommandParams p)
        {
            // Excelファイルのユースケースシナリオを読み込み
            var filter = "Excel Files (*.xls, *.xlsx)|*.xls;*.xlsx";
            var filePath = m_Context.App.Window.UI.ShowOpenFileDialog(filter: filter);
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            var scFactory = new UCScenarioFactory();
            var scenario = scFactory.create(filePath);

            // モデルに書き込み
            var project = App.Workspace.CurrentProject;
            var pkg = findPackage(project, "システム開発/システム要件開発/ユースケース");
            var builder = new UCScenarioBuilder();
            builder.AddScenario(pkg, scenario);

            App.Window.UI.ShowMessageBox(scenario.ScenarioId, "LoadUCScenario");
        }

        #endregion

    }
}
