using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BatDongSan.Models.DanhMuc;
using BatDongSan.Models.HeThong;

namespace BatDongSan.Models.NhanSu
{
    public class PhuLucHopDongModel
    {

        public int id { get; set; }

        public string soPhuLuc { get; set; }

        public string soHopDong { get; set; }

        public string maNhanVien { get; set; }

        public string tenNhanVien { get; set; }

        public string tenChucDanh { get; set; }
        public string maLoaiHopDong { get; set; }

        public string tenLoaiHopDong { get; set; }
        public int idHopDong { get; set; }

        public string noiDungThayDoi { get; set; }

        public System.Nullable<System.DateTime> ngayHieuLuc { get; set; }
        public System.Nullable<System.DateTime> giaHanDen { get; set; }
        public System.Nullable<System.DateTime> ngayBatDauTinhPhep { get; set; }

        public System.Nullable<decimal> soLuongCu { get; set; }

        public System.Nullable<decimal> mucDieuChinh { get; set; }

        public System.Nullable<decimal> mucLuongMoi { get; set; }

        public string ghiChu { get; set; }

        public string nguoiLap { get; set; }

        public string nguoiCapNhat { get; set; }

        public System.Nullable<System.DateTime> ngayLap { get; set; }

        public int maQuiTrinhDuyet { get; set; }

        public DMNguoiDuyet Duyet { get; set; }

        public decimal luongDongBH { get; set; }

        public decimal khoanBoSungLuong { get; set; }

        public double doanPhi { get; set; }

        public double dangPhi { get; set; }

        public bool tienAnGiuaCa { get; set; }

        public bool checkPhuCapDiLai { get; set; }
        public decimal tienDienThoai { get; set; }

        public decimal luong { get; set; }

        public double phuCapLuong { get; set; }

        public decimal tongLuong { get; set; }

        public int? bac { get; set; }
        public string maHinhThuc { get; set; }

        public string tenHinhThuc { get; set; }
        public string maThue { get; set; }

        public string tenThue { get; set; }

        public string maBaoHiem { get; set; }

        public string tenBaoHiem { get; set; }
        public decimal phuCapDiLaiNew { get; set; }
        public int? bacChucVu { get; set; }

        public System.Nullable<decimal> luongCoBan { get; set; }

        public System.Nullable<decimal> luongThanhTich { get; set; }

        public System.Nullable<decimal> tienXang { get; set; }

        public System.Nullable<decimal> tienDiLai { get; set; }

        public List<tbl_DM_ThangLuongCongTy> thangLuongs { get; set; }

        public PhuLucHopDongModel()
        {
            Duyet = new DMNguoiDuyet();
            thangLuongs = new List<tbl_DM_ThangLuongCongTy>();
        }

        public int duocPhepTao { get; set; }

        public string maPhuLucCu { get; set; }

        public string thue { get; set; }

        public string baoHien { get; set; }

        public string hinhThucLuong { get; set; }

        public string maLoaiHopDongPL { get; set; }
    }
}