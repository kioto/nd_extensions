using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;

namespace LoadUCScenario
{
    public class UCScenarioFactory
    {
        static int MAX_ROW_INDEX = 1000;  // 行番号の最大値

        public UCScenarioFactory()
        {
        }

        public UCScenario create(string filePath)
        {
            var workbook = WorkbookFactory.Create(filePath);
            var worksheet = workbook.GetSheetAt(0);
            var scenario = new UCScenario();

            SetHeader(scenario, worksheet);
            SetFlows(scenario, worksheet);

            return scenario;
        }

        private void SetHeader(UCScenario scenario, ISheet sheet)
        {
            var rowIndex = 0;
            while (true)
            {
                var row = sheet.GetRow(rowIndex);
                var title = row.GetCell(0).ToString();
                var opt = row.GetCell(1).ToString();
                var val = row.GetCell(2).ToString();
                bool preCond = false;
                bool postCond = false;
                if (title == "" && opt == "")
                {
                    break;
                }

                switch (title)
                {
                    case "シナリオID":
                        scenario.ScenarioId = val;
                        break;
                    case "関連要求ID":
                        scenario.RelatedRequirementId = val;
                        break;
                    case "概要、場面":
                        scenario.Abstruct = val;
                        break;
                    case "アクター":
                        scenario.Actors = val.Split('\n');
                        break;
                    case "ステークホルダ要求":
                        scenario.StakeholderRequiement = val;
                        break;
                    case "関連要求（制約）":
                        scenario.RelatedRequirement = val;
                        break;
                    case "事前条件":
                        preCond = true;
                        postCond = false;
                        break;
                    case "事後条件":
                        preCond = false;
                        postCond = true;
                        break;
                    default:
                        break;
                }

                if (opt == "")
                {
                    preCond = false;
                    postCond = false;
                }
                else if (preCond)
                {
                    // 事前条件
                    var cond = new LoadUCScenario.UCScenarioCondition(opt, val);
                    scenario.PreConditions.Add(cond);
                }
                else if (postCond)
                {
                    // 事後条件
                    var cond = new LoadUCScenario.UCScenarioCondition(opt, val);
                    scenario.PostConditions.Add(cond);
                }

                rowIndex++;
            }
            return;
        }

        private int GetFlowHeaderIndex(ISheet sheet)
        {
            for (var rowIndex = 0; rowIndex < MAX_ROW_INDEX; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row.GetCell(0).ToString() == "フロー")
                {
                    return rowIndex;
                }
            }

            return -1;
        }

        private struct FlowRow
        {
            public string FlowType = "";
            public string FlowId = "";
            public string Scenario = "";
            public string[] Branches = new string[0];
            public string Note = "";

            public FlowRow(IRow row)
            {
                var flowType = row.GetCell(0).ToString();
                FlowType = flowType;
                FlowId = row.GetCell(2).ToString();
                Scenario = row.GetCell(3).ToString();
                Branches = row.GetCell(4).ToString().Split('\n');
                Note = row.GetCell(5).ToString();
            }

            public bool isMainFlow()
            {
                return FlowType == "基本フロー";
            }

            public bool isAlternativeFlows()
            {
                return FlowType == "代替フロー";
            }
            public bool isExceptionFlows()
            {
                return FlowType == "例外フロー";
            }
            public bool isOtherFlowType()
            {
                return FlowType.Length > 0 && !isMainFlow() && !isAlternativeFlows() && !isExceptionFlows();
            }

            public bool isFlowTitle()
            {
                return FlowId.Length > 0 && !FlowId.Contains("-");
            }
        }

        private enum FlowTypeEnum
        {
            None,
            MainFlow,
            AlternativeFlow,
            ExceptionFlow,
        }

        private void EntryFlowElement(UCScenarioFlow flow, FlowRow row)
        {
            UCScenarioFlowElememnt elem = new UCScenarioFlowElememnt(row.FlowId, row.Scenario, row.Branches, row.Note);
            flow.Sequence.Add(elem);
        }

        private void SetFlows(UCScenario scenario, ISheet sheet)
        {
            // 「フロー」のヘッダまでスキップ
            var rowIndex = GetFlowHeaderIndex(sheet);
            if (rowIndex < 0) return;  // フローヘッダが見つからない

            // フロー行の解析
            FlowTypeEnum flowType = FlowTypeEnum.None;
            UCScenarioFlow flow = new UCScenarioFlow();
            rowIndex++;  // ヘッダの次の行から開始
            for (; rowIndex < MAX_ROW_INDEX; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null) break;

                bool isFlowTitle = false;
                var flowRow = new FlowRow(row);
                // 基本/代替/例外フローのステート切り替え
                if (flowRow.isMainFlow())
                {
                    flowType = FlowTypeEnum.MainFlow;
                    isFlowTitle = true;  // 基本フローだけ無条件に先頭
                    flow = new UCScenarioFlow("基本フロー", "MF", "基本フロー");
                    scenario.MainFlow = flow;
                }
                else if (flowRow.isAlternativeFlows())
                {
                    flowType = FlowTypeEnum.AlternativeFlow;
                }
                else if (flowRow.isExceptionFlows())
                {
                    flowType = FlowTypeEnum.ExceptionFlow;
                } else if (flowRow.isOtherFlowType())
                {
                    flowType = FlowTypeEnum.None;
                    continue;
                }

                // フローの先頭判定
                if (isFlowTitle == false)
                {
                    if (flowType == FlowTypeEnum.AlternativeFlow && flowRow.isFlowTitle())
                    {
                        flow = new UCScenarioFlow("代替フロー", flowRow.FlowId, flowRow.Scenario);
                        scenario.AlternativeFlows.Add(flow);
                        continue;
                    }
                    else if (flowType == FlowTypeEnum.ExceptionFlow && flowRow.isFlowTitle())
                    {
                        flow = new UCScenarioFlow("例外フロー", flowRow.FlowId, flowRow.Scenario);
                        scenario.ExceptionFlows.Add(flow);
                        continue;
                    }
                }

                if (flowType != FlowTypeEnum.None)
                {
                    // フローを登録
                    EntryFlowElement(flow, flowRow);
                }
            }
        }
    }
}
