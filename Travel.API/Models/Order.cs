using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stateless;

namespace Travel.API.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public ICollection<LineItem> OrderItems { get; set; }

        public OrderStateEnum OrderState { get; set; }

        public DateTime CreateDateUTC { get; set; }

        public string TransactionMetadata { get; set; }//第三方回调数据

        StateMachine<OrderStateEnum, OrderStateTriggerEnum> _machine; //状态机

        public Order()
        {
            StateMachineInit();
        }

        private void StateMachineInit()
        {
            _machine = new StateMachine<OrderStateEnum, OrderStateTriggerEnum>(() => OrderState, s => OrderState = s);

            // 配置触发动作以及状态转换
            _machine.Configure(OrderStateEnum.Pending)
                .Permit(OrderStateTriggerEnum.PlaceOrder, OrderStateEnum.Processing)
                .Permit(OrderStateTriggerEnum.Cancel, OrderStateEnum.Cancelled);

            _machine.Configure(OrderStateEnum.Processing)
                .Permit(OrderStateTriggerEnum.Approve, OrderStateEnum.Completed)
                .Permit(OrderStateTriggerEnum.Reject, OrderStateEnum.Declined);

            _machine.Configure(OrderStateEnum.Declined)
                .Permit(OrderStateTriggerEnum.PlaceOrder, OrderStateEnum.Processing);

            _machine.Configure(OrderStateEnum.Completed)
                .Permit(OrderStateTriggerEnum.Return, OrderStateEnum.Refund);
        }

        public void PaymentProcessing()
        {
            _machine.Fire(OrderStateTriggerEnum.PlaceOrder);
        }

        public void PaymentApprove()
        {
            _machine.Fire(OrderStateTriggerEnum.Approve);
        }

        public void PaymentReject()
        {
            _machine.Fire(OrderStateTriggerEnum.Reject);
        }
    }

    public enum OrderStateEnum
    {
        Pending,   //订单已生成
        Processing,//制服处理中
        Completed, //交易成功
        Declined,  //交易失败
        Cancelled, //订单取消
        Refund     //已退款
    }

    //订单状态触发
    public enum OrderStateTriggerEnum
    {
        PlaceOrder, //支付
        Approve,    //支付成功
        Reject,     //支付失败
        Cancel,     //取消
        Return      //退货
    }

}
