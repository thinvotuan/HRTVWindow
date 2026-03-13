using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BatDongSan.Models.HeThong;
using BatDongSan.Models.NhanSu;

namespace BatDongSan.Models.NhanSu
{
    public class TiLeThanhTich
    {
        public string maPhieu { get; set; }

        public string maPhongBan { get; set; }

        public string tenPhongBan { get; set; }

        public string tenChucDanh { get; set; }


        public bool xacNhan { get; set; }

        public string noiDung { get; set; }

        public string nguoiLap { get; set; }

        public System.Nullable<System.DateTime> ngayLap { get; set; }



        public NhanVienModel NhanVien { get; set; }




        public double tyle { get; set; }

        public int quy { get; set; }
        public int nam { get; set; }


        public string maNhanVien { get; set; }

        public double? luongThanhTich { get; set; }

        public double? tienThue { get; set; }

        public double? thucNhan { get; set; }

        public int? thang { get; set; }
        public string hoVaTen { get; set; }

        public string email { get; set; }
    }
}