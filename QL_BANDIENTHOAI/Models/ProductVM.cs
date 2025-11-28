using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_BANDIENTHOAI.Models
{
    public class ProductVM
    {
        public SanPham SP { get; set; }

        // Số sao trung bình
        public double AvgStar { get; set; }

        // Tổng số đánh giá
        public int CountStar { get; set; }
    }
}