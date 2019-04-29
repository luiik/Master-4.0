﻿using System.Collections.Generic;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_PurchaseOrderPart : BaseEntity, IDemand
    {
        public int PurchaseId { get; set; }
        [JsonIgnore]
        public T_PurchaseOrder Purchase { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public State State { get; set; }
        public int DemandID { get; set; }
        public T_Demand Demand { get; set; }
    }
}