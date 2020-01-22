﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibflClassLibrary.ImageCatalog
{
    public class ICLoader
    {
        string tableName_;
        internal ImageCard GetCard(string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
            {
                throw new Exception("M001");
            }
            int cardIdNumeric;
            bool tryNumber = int.TryParse(cardId, out cardIdNumeric);
            if (!tryNumber)
            {
                throw new Exception("M002");
            }
            ImageCard result = new ImageCard();
            string firstNumber = cardId.Substring(0, 1);
            switch (firstNumber)
            {
                case 0:
                    this.tableName_ = "CardMain";
                    break;
                case 1:
                    this.tableName_ = "CardPeriodical";
            }
            
        }
    }
}
