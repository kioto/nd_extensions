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
        private void AddFlow(IModel usecase, UCScenarioFlow ucsFlow)
        {
            var flow = usecase.AddNewModel("Scenarios", "フロー");
            if (ucsFlow.FlowType == "基本フロー")
            {
                flow.SetField("Name", ucsFlow.FlowType);
            } else
            {
                flow.SetField("Name", ucsFlow.Description);
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

        public void AddScenario(IModel model, UCScenario ucs)
        {
            // ユースケースのモデルを作成
            var uc = model.AddNewModel("OwnedElements", "Usecase");
            uc.SetField("Name", ucs.ScenarioId);
            uc.SetField("Description", ucs.ScenarioId);

            // 基本フローの登録
            AddFlow(uc, ucs.MainFlow);

            // 代替フローの登録
            foreach (var flow in ucs.AlternativeFlows)
            {
                AddFlow(uc, flow);
            }

            // 例外フローの登録
            foreach (var flow in ucs.ExceptionFlows)
            {
                AddFlow(uc, flow);
            }
        }
    }
}
