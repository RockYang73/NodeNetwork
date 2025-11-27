using ActuatorApp.Core.Enums;
using ActuatorApp.Core.Interfaces;

namespace ActuatorApp.Core.Validation
{
    /// <summary>
    /// 連線規則引擎 - 純業務邏輯，與 UI 框架無關
    /// </summary>
    public class ConnectionRuleEngine : IConnectionValidator
    {
        private string _lastErrorMessage;

        /// <summary>
        /// 驗證連線是否有效
        /// </summary>
        public ConnectionValidationResult Validate(
            IActuatorNode sourceNode,
            IActuatorPort sourcePort,
            IActuatorNode targetNode,
            IActuatorPort targetPort)
        {
            // 基本檢查
            if (sourceNode == null || sourcePort == null || targetNode == null || targetPort == null)
            {
                return ConnectionValidationResult.Failure("無效的連線端點");
            }

            // 類型必須匹配
            if (sourcePort.PortType != targetPort.PortType)
            {
                return ConnectionValidationResult.Failure(
                    $"端口類型不匹配: {sourcePort.PortType} → {targetPort.PortType}");
            }

            // 不能連接到自己
            if (ReferenceEquals(sourceNode, targetNode))
            {
                return ConnectionValidationResult.Failure("不能連接到自己");
            }

            // 根據端口類型進行特定驗證
            switch (sourcePort.PortType)
            {
                case PortType.Power:
                    return ValidatePowerConnection(sourceNode, targetNode);

                case PortType.BinaryData:
                    return ValidateBinaryDataConnection(sourceNode, targetNode);

                case PortType.Parameter:
                    return ValidateParameterConnection(sourceNode, targetNode);

                default:
                    return ConnectionValidationResult.Success();
            }
        }

        /// <summary>
        /// 驗證電流連線
        /// 允許: TP → TBB, TBB → TC
        /// </summary>
        private ConnectionValidationResult ValidatePowerConnection(
            IActuatorNode sourceNode, IActuatorNode targetNode)
        {
            // TP.Out → TBB.In
            if (sourceNode.NodeType == NodeType.PowerSupply && 
                targetNode.NodeType == NodeType.Battery)
            {
                return ConnectionValidationResult.Success();
            }

            // TBB.Out → TC.Power
            if (sourceNode.NodeType == NodeType.Battery && 
                targetNode.NodeType == NodeType.ControlBox)
            {
                return ConnectionValidationResult.Success();
            }

            return ConnectionValidationResult.Failure("電流連接順序: TP → TBB → TC");
        }

        /// <summary>
        /// 驗證二進制信號連線
        /// 允許: 
        /// - TC.M → TA (Actuator 控制)
        /// - Control (TH/TFH/THP) → TC.H1 (直連)
        /// - Control (TH/TFH/THP) → TYC.IN (經由延長線/分接線)
        /// - TYC.Out → TC.H1 (配件連接控制盒)
        /// - TYC.Out → TYC.IN (配件級聯)
        /// </summary>
        private ConnectionValidationResult ValidateBinaryDataConnection(
            IActuatorNode sourceNode, IActuatorNode targetNode)
        {
            var sourceType = sourceNode.NodeType;
            var targetType = targetNode.NodeType;

            // TC.M → TA.TC (控制盒控制推桿)
            if (sourceType == NodeType.ControlBox && targetType == NodeType.Actuator)
            {
                return ConnectionValidationResult.Success();
            }

            // Control → TC.H1 (手控器直連控制盒)
            if (sourceType == NodeType.Control && targetType == NodeType.ControlBox)
            {
                return ConnectionValidationResult.Success();
            }

            // Control → TYC.IN (手控器連接延長線/分接線)
            if (sourceType == NodeType.Control && targetType == NodeType.Accessory)
            {
                return ConnectionValidationResult.Success();
            }

            // TYC.Out → TC.H1 (配件連接控制盒)
            if (sourceType == NodeType.Accessory && targetType == NodeType.ControlBox)
            {
                return ConnectionValidationResult.Success();
            }

            // TYC.Out → TYC.IN (配件級聯)
            if (sourceType == NodeType.Accessory && targetType == NodeType.Accessory)
            {
                return ConnectionValidationResult.Success();
            }

            return ConnectionValidationResult.Failure(
                "信號連接無效。允許: TC→TA, Control→TC, Control→TYC, TYC→TC, TYC→TYC");
        }

        /// <summary>
        /// 驗證參數連線
        /// 允許: Parameter → TA (Code/Stroke)
        /// </summary>
        private ConnectionValidationResult ValidateParameterConnection(
            IActuatorNode sourceNode, IActuatorNode targetNode)
        {
            // Parameter → TA
            if (sourceNode.NodeType == NodeType.Parameter && 
                targetNode.NodeType == NodeType.Actuator)
            {
                return ConnectionValidationResult.Success();
            }

            // Parameter 可以連接到任何接受 Parameter 的節點
            if (sourceNode.NodeType == NodeType.Parameter)
            {
                return ConnectionValidationResult.Success();
            }

            return ConnectionValidationResult.Failure("參數連接無效");
        }
    }
}
