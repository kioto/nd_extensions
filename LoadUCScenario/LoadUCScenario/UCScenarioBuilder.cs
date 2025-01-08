using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextDesign.Core;

namespace LoadUCScenario
{
    internal class UCScenarioBuilder
    {
        private void AddActor(IModel actor_package, IModel usecase, String actorName)
        {

            IModel actor = NDTools.findActor(actor_package, actorName);
            if (actor == null)
            {
                actor = actor_package.AddNewModel("OwnedElements", "Actor");
                actor.SetField("Name", actorName);
            }
            usecase.Relate("Actors", actor);
        }

        private void AddFlow(IModel usecase, UCScenarioFlow ucsFlow)
        {
            var flow = usecase.AddNewModel("Scenarios", "フロー");
            if (ucsFlow.FlowType == "基本フロー")
            {
                flow.SetField("Name", ucsFlow.FlowType);
            } else
            {
                flow.SetField("Name", ucsFlow.Description);
                flow.SetField("フロー区分", ucsFlow.FlowType);
            }
            flow.SetField("Description", ucsFlow.Description);
            flow.SetField("ID", ucsFlow.FlowId);

            foreach(var elem in ucsFlow.Sequence)
            {
                var step = flow.AddNewModel("Steps", "Step");
                step.SetField("フローID", elem.FlowId);
                step.SetField("シナリオ", elem.Scenario);
                step.SetField("Name", elem.Scenario);
                step.SetField("分岐", String.Join("\n", elem.Branches));
                step.SetField("備考", elem.Note);
            }
        }

        public void AddScenario(IProject project, UCScenario ucs)
        {
            // ユースケースパッケージの取得
            var uc_package = NDTools.findPackage(project, "システム開発/システム要件開発/ユースケース");

            // ユースケースのモデルを作成
            var uc_model = uc_package.AddNewModel("OwnedElements", "Usecase");
            uc_model.SetField("Name", ucs.ScenarioId);
            uc_model.SetField("Description", ucs.ScenarioId);

            // アクターパッケージの取得
            var actor_package = NDTools.findPackage(project, "システム開発/システム要件開発/アクター");

            // アクターの登録
            foreach (var actorName in ucs.Actors)
            {
                AddActor(actor_package, uc_model, actorName);
            }

            // 基本フローの登録
            AddFlow(uc_model, ucs.MainFlow);

            // 代替フローの登録
            foreach (var flow in ucs.AlternativeFlows)
            {
                AddFlow(uc_model, flow);
            }

            // 例外フローの登録
            foreach (var flow in ucs.ExceptionFlows)
            {
                AddFlow(uc_model, flow);
            }
        }
    }
}
