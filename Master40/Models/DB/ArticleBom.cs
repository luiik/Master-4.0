﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.Models.DB
{
    public class ArticleBom
    {
        [Key]
        public int ArticleBomId { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public ICollection<ArticleBomItem> ArticleBomItems { get; set; }
        public string Name { get; set; }

    }
}
