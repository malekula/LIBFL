﻿using LibflClassLibrary.BJUsers;
using LibflClassLibrary.Circulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibflClassLibrary.ImageCatalog
{
    public class ImageCatalogCirculationManager
    {
        public List<ICOrderInfo> GetActiveOrdersByReader(int readerId)
        {
            ICOrderLoader loader = new ICOrderLoader();
            return loader.GetActiveOrdersByReader(readerId);
        }

        public void DeleteOrder(int readerId, int orderId)
        {
            ICOrderLoader loader = new ICOrderLoader();
            ICOrderInfo order = ICOrderInfo.GetICOrderById(orderId);
            if (order.StatusName != CirculationStatuses.OrderIsFormed.Value)
            {
                throw new Exception("M005");
            }
            loader.DeleteOrder(readerId, orderId);
            return;
        }

        public List<ICOrderInfo> GetHistoryOrdersByReader(int readerId)
        {
            ICOrderLoader loader = new ICOrderLoader();
            return loader.GetHistoryOrdersByReader(readerId);
        }
        public void RefuseOrder(ICOrderInfo order, BJUserInfo bjUser, string refusualReason)
        {
            ICOrderLoader loader = new ICOrderLoader();
            loader.RefuseOrder(order, bjUser, refusualReason);
        }
    }
}