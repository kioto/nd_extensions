using System;
using System.Collections.Generic;
using System.IO.Packaging;
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

        private void AddPreCondition(IModel usecase, UCScenarioCondition ucsCond)
        {
            var cond = usecase.AddNewModel("PreCondition", "Condition");
            cond.SetField("Name", ucsCond.Title);
            cond.SetField("Description", ucsCond.Condition);
        }

        private void AddPostCondition(IModel usecase, UCScenarioCondition ucsCond)
        {
            var cond = usecase.AddNewModel("PostCondition", "Condition");
            cond.SetField("Name", ucsCond.Title);
            cond.SetField("Description", ucsCond.Condition);
        }

        private (IModel, string) AddFlow(IModel usecase, UCScenarioFlow ucsFlow)
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
                //step.SetField("分岐", String.Join("\n", elem. ));
                step.SetField("備考", elem.Note);
            }

            return (flow, ucsFlow.FlowId);
        }

        private IModel FindStepById(IModel flow, string stepId)
        {
            foreach (var step in flow.GetFieldValues("Steps"))
            {
                if (stepId == step.GetFieldString("フローID")) {
                    return step;
                }
            }

            return null;
        }

        private void entryBranch(UCScenarioFlow ucsFlow, Dictionary<string, IModel> flowDic)
        {
            var flow = flowDic[ucsFlow.FlowId];
            foreach (var elem in ucsFlow.Sequence)
            {
                if (elem.Branches.Length > 0) {
                    var targetStep = FindStepById(flow, elem.FlowId);
                    foreach (var sid in elem.Branches)
                    {
                        var branchFlow = flowDic[sid];
                        targetStep.Relate("Branches", branchFlow);
                    }
                }
            }
        }

        private void AddIssue(IModel usecase, UCScenarioIssue issue)
        {
            var cond = usecase.AddNewModel("Issues", "Issue");
            cond.SetField("Name", issue.IssueId);
            cond.SetField("Description", issue.Description);
        }

        public void AddScenario(IProject project, UCScenario ucs)
        {
            // ユースケースパッケージの取得
            var uc_package = NDTools.findPackage(project, "ユースケース");
            if(uc_package == null)
            {
                uc_package = project.AddNewRootModel("Package");
                uc_package.SetField("Name", "ユースケース");
            }

            // ユースケースのモデルを作成
            var uc_model = uc_package.AddNewModel("OwnedElements", "Usecase");
            uc_model.SetField("Name", ucs.ScenarioId);
            uc_model.SetField("Description", ucs.ScenarioId);
            uc_model.SetField("RelatedRequirementId", ucs.RelatedRequirementId);
            uc_model.SetField("Abstract", ucs.Abstract);
            uc_model.SetField("StakeholderRequirement", ucs.StakeholderRequiement);
            uc_model.SetField("RelatedRequirement", ucs.RelatedRequirement);

            // アクターパッケージの取得
            var actor_package = NDTools.findPackage(project, "アクター");
            if (actor_package == null)
            {
                actor_package = project.AddNewRootModel("Package");
                actor_package.SetField("Name", "アクター");
            }

            // アクターの登録
            foreach (var actorName in ucs.Actors)
            {
                if(actorName.StartsWith(" ") ||
                    actorName.StartsWith("　") ||
                    actorName.StartsWith("※")) {
                    continue;
                }
                AddActor(actor_package, uc_model, actorName);
            }

            // 事前条件の登録
            foreach(var cond in ucs.PreConditions)
            {
                AddPreCondition(uc_model, cond);
            }

            // 事後条件の登録
            foreach (var cond in ucs.PostConditions)
            {
                AddPostCondition(uc_model, cond);
            }

            // フローIDとフローの辞書
            var flowDic = new Dictionary<string, IModel>();
            IModel res = null;
            string resId = "";

            // 基本フローの登録
            (res, resId) = AddFlow(uc_model, ucs.MainFlow);
            flowDic.Add(resId, res);

            // 代替フローの登録
            foreach (var flow in ucs.AlternativeFlows)
            {
                (res, resId) = AddFlow(uc_model, flow);
                flowDic.Add(resId, res);
            }

            // 例外フローの登録
            foreach (var flow in ucs.ExceptionFlows)
            {
                (res, resId) = AddFlow(uc_model, flow);
                flowDic.Add(resId, res);
            }

            // ステップの分岐先フローを登録
            entryBranch(ucs.MainFlow, flowDic);
            foreach (var flow in ucs.AlternativeFlows)
            {
                entryBranch(flow, flowDic);
            }
            foreach (var flow in ucs.ExceptionFlows)
            {
                entryBranch(flow, flowDic);
            }

            // 課題、TBD事項を登録
            foreach (var issue in ucs.Issues)
            {
                AddIssue(uc_model, issue);
            }
        }
    }
}
