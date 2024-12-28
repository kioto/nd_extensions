using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LoadUCScenario
{
    public struct UCScenarioCondition
    {
        public string Actor;      // アクター
        public string Condition;  // 条件

        public UCScenarioCondition(string actor, string condition)
        {
            Actor = actor;
            Condition = condition;
        }
    }

    public struct UCScenarioFlowElememnt
    {
        public string FlowId;
        public string Scenario;
        public string[] Branches;
        public string Note;

        public UCScenarioFlowElememnt(string flowid, string scenario, string[] branches, string note)
        {
            FlowId = flowid;
            Scenario = scenario;
            Branches = branches;
            Note = note;
        }
    }

    public struct UCScenarioFlow
    {
        public string FlowType;
        public string FlowId;
        public string Description;
        public List<UCScenarioFlowElememnt> Sequence;

        public UCScenarioFlow(string flowType, string flowId, string desc)
        {
            FlowType = flowType;
            FlowId = flowId;
            Description = desc;
            Sequence = new List<UCScenarioFlowElememnt>();
        }
    }

    public class UCScenario
    {
        public string ScenarioId;                  // シナリオID
        public string RelatedRequirementId;       // 関連要求ID
        public string Abstruct;                   // 概要、場面
        public string[] Actors;                   //.アクター
        public string StakeholderRequiement;      // ステークホルダー要求
        public string RelatedRequirement;         // 関連要求
        public List<UCScenarioCondition> PreConditions;   // 事前条件
        public List<UCScenarioCondition> PostConditions;  // 事後条件

        public UCScenarioFlow MainFlow;                // 基本フロー
        public List<UCScenarioFlow> AlternativeFlows;  // 代替フロー
        public List<UCScenarioFlow> ExceptionFlows;    // 例外フロー

        public UCScenario()
        {
            ScenarioId = "";
            RelatedRequirementId = "";
            Abstruct = "";
            Actors = new string[0];
            StakeholderRequiement = "";
            RelatedRequirement = "";
            PreConditions = new List<UCScenarioCondition>();
            PostConditions = new List<UCScenarioCondition>();

            MainFlow = new UCScenarioFlow();
            AlternativeFlows = new List<UCScenarioFlow>();
            ExceptionFlows = new List<UCScenarioFlow>();
        }
    }
}
