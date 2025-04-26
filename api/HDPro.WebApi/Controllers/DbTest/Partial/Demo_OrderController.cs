/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Demo_Order",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.DbTest.IServices;
using HDPro.Core.Enums;
using HDPro.Core.Filters;
using HDPro.DbTest.IRepositories;
using Microsoft.EntityFrameworkCore;
using HDPro.Core.Extensions;
using System.Linq;
using HDPro.Core.Tenancy;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace HDPro.DbTest.Controllers
{
    public partial class Demo_OrderController
    {
        private readonly IDemo_OrderService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IDemo_GoodsService _goodsService;//商品信息业务类

        private readonly IDemo_OrderListRepository _orderListRepository;
        private readonly IDemo_OrderRepository _orderRepository;
        [ActivatorUtilitiesConstructor]
        public Demo_OrderController(
            IDemo_OrderService service,
            IHttpContextAccessor httpContextAccessor,
            IDemo_GoodsService goodsService,
            IDemo_OrderListRepository orderListRepository,
            IDemo_OrderRepository orderRepository

        )
        : base(service)
        {
            _service = service;
            _orderRepository = orderRepository;
            _goodsService = goodsService;
            _orderListRepository = orderListRepository;
            _httpContextAccessor = httpContextAccessor;
        }


        public override ActionResult GetPageData([FromBody] PageDataOptions loadData)
        {
            return base.GetPageData(loadData);
        }

        [HttpGet, Route("test1")]
        public IActionResult Test1()
        {
            return Content("test1");
        }

        //批量选择获取明商品数据
        [Route("getGoods"), HttpPost]
        public IActionResult GetGoods([FromBody] PageDataOptions loadData)
        {
            //调用商品信息的查询方法
            var gridData = _goodsService.GetPageData(loadData);

            return JsonNormal(gridData);
        }
        /// <summary>
        /// 获取订单明细数据
        /// </summary>
        /// <param name="Order_Id"></param>
        /// <returns></returns>
        [Route("getDetailRows"), HttpGet]
        public async Task<IActionResult> GetDetailRows(Guid Order_Id)
        {
            var rows = await _orderListRepository.FindAsIQueryable(x => x.Order_Id == Order_Id)
                  .ToListAsync();
            return JsonNormal(rows);
        }

        /// <summary>
        /// 获取订单明细数据
        /// </summary>
        /// <param name="Order_Id"></param>
        /// <returns></returns>
        [Route("submitAudit"), HttpPost]
        public IActionResult SubmitAudit([FromBody] object[] ids)
        {
            var result = Service.SubmitWorkFlowAudit(ids);
            return Json(result);
        }

        /// <summary>
        /// 获取订单明细数据
        /// </summary>
        /// <param name="Order_Id"></param>
        /// <returns></returns>
        [Route("getTotal"), HttpGet]
        public async Task<IActionResult> GetTotal()
        {
          //获取汇总
          var total=  await _orderRepository.FindAsIQueryable(x => true)
                  .GroupBy(x => true).
                  Select(x => new
                  {
                      orderType = -1,
                      count = x.Count(),
                      qty = x.Sum(c => c.TotalQty),
                      totalPrice = x.Sum(c => c.TotalPrice),
                  }).FirstAsync();
            //获取每个订单类型数据
            var data = await _orderRepository.FindAsIQueryable(x => true)
                   .GroupBy(x => x.OrderType).
                   Select(x => new
                   {
                       orderType = x.Key,
                       count = x.Count(),
                       qty = x.Sum(c => c.TotalQty),
                       totalPrice = x.Sum(c => c.TotalPrice),
                   }).ToListAsync();

            List<object> list = new List<object>() { total };
            list.AddRange(data);

            return Json(list);
        }


    }
}
