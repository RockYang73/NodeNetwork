using ActuatorApp.ViewModels;
using ActuatorApp.ViewModels.Nodes;
using NodeNetwork;
using NodeNetwork.ViewModels;

namespace ActuatorApp.Validation
{
    /// <summary>
    /// 連線驗證規則
    /// </summary>
    public static class ConnectionRules
    {
        /// <summary>
        /// 驗證連線是否有效
        /// </summary>
        public static ConnectionValidationResult ValidateConnection(
            PendingConnectionViewModel pending,
            PortType expectedType)
        {
            var output = pending.Output;

            if (output == null)
                return new ConnectionValidationResult(false,
                    new ErrorMessageViewModel("無效的輸出端口"));

            // 取得輸出端口類型
            var outputPort = output.Port as ActuatorPortViewModel;
            if (outputPort == null)
                return new ConnectionValidationResult(false,
                    new ErrorMessageViewModel("無效的端口類型"));

            // 類型必須匹配
            if (outputPort.PortType != expectedType)
                return new ConnectionValidationResult(false,
                    new ErrorMessageViewModel($"端口類型不匹配: 預期 {expectedType}, 實際 {outputPort.PortType}"));

            // 取得節點類型進行流向驗證
            var inputNode = pending.Input?.Parent;
            var outputNode = output.Parent;

            // Power 流向驗證
            if (expectedType == PortType.Power)
            {
                return ValidatePowerFlow(inputNode, outputNode);
            }

            // BinaryData 流向驗證
            if (expectedType == PortType.BinaryData)
            {
                return ValidateBinaryDataFlow(inputNode, outputNode);
            }

            // Parameter 流向驗證 (較寬鬆)
            if (expectedType == PortType.Parameter)
            {
                return ValidateParameterFlow(inputNode, outputNode);
            }

            return new ConnectionValidationResult(true, null);
        }

        /// <summary>
        /// 驗證電流流向
        /// 允許: TP → TBB, TBB → TC
        /// </summary>
        private static ConnectionValidationResult ValidatePowerFlow(
            NodeViewModel inputNode, NodeViewModel outputNode)
        {
            // TP.Out → TBB.In
            if (outputNode is TPNodeViewModel && inputNode is TBBNodeViewModel)
                return new ConnectionValidationResult(true, null);

            // TBB.Out → TC.Power
            if (outputNode is TBBNodeViewModel && inputNode is TCNodeViewModel)
                return new ConnectionValidationResult(true, null);

            return new ConnectionValidationResult(false,
                new ErrorMessageViewModel("電流連接順序: TP → TBB → TC"));
        }

        /// <summary>
        /// 驗證二進制信號流向
        /// 允許: 
        /// - TC.M → TA (Actuator 控制)
        /// - Control (TH/TFH/THP) → TC.H1 (直連)
        /// - Control (TH/TFH/THP) → TYC.IN (經由延長線/分接線)
        /// - TYC.Out → TC.H1 (配件連接控制盒)
        /// - TYC.Out → TYC.IN (配件級聯)
        /// </summary>
        private static ConnectionValidationResult ValidateBinaryDataFlow(
            NodeViewModel inputNode, NodeViewModel outputNode)
        {
            // TC.M → TA.TC
            if (outputNode is TCNodeViewModel && inputNode is TANodeViewModel)
                return new ConnectionValidationResult(true, null);

            // Control (TH/TFH/THP) → TC.H1 (直連)
            if (outputNode is THNodeViewModel && inputNode is TCNodeViewModel)
                return new ConnectionValidationResult(true, null);
            if (outputNode is THPNodeViewModel && inputNode is TCNodeViewModel)
                return new ConnectionValidationResult(true, null);

            // Control (TH/TFH/THP) → TYC.IN (手控器連接到延長線/分接線)
            if (outputNode is THNodeViewModel && inputNode is TYCNodeViewModel)
                return new ConnectionValidationResult(true, null);
            if (outputNode is THPNodeViewModel && inputNode is TYCNodeViewModel)
                return new ConnectionValidationResult(true, null);

            // TYC.Out → TC.H1 (配件連接控制盒)
            if (outputNode is TYCNodeViewModel && inputNode is TCNodeViewModel)
                return new ConnectionValidationResult(true, null);

            // TYC.Out → TYC.IN (配件級聯)
            if (outputNode is TYCNodeViewModel && inputNode is TYCNodeViewModel)
                return new ConnectionValidationResult(true, null);

            return new ConnectionValidationResult(false,
                new ErrorMessageViewModel("信號連接無效。允許: TC→TA, Control→TC, Control→TYC, TYC→TC, TYC→TYC"));
        }

        /// <summary>
        /// 驗證參數流向
        /// 允許: Parameter → TA (Code/Stroke) 等
        /// </summary>
        private static ConnectionValidationResult ValidateParameterFlow(
            NodeViewModel inputNode, NodeViewModel outputNode)
        {
            // Parameter → TA
            if (outputNode is ParameterNodeViewModel && inputNode is TANodeViewModel)
                return new ConnectionValidationResult(true, null);

            // 其他參數連接 (較寬鬆，允許 Parameter 連接到任何接受 Parameter 的節點)
            if (outputNode is ParameterNodeViewModel)
                return new ConnectionValidationResult(true, null);

            return new ConnectionValidationResult(false,
                new ErrorMessageViewModel("參數連接無效"));
        }
    }
}
