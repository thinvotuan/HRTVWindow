using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Common;
using BatDongSan.Utils.Paging;
using BatDongSan.Helper.Utils;
using System.Data;
using System.Globalization;
using System.Text;
using System.IO;
using BatDongSan.Models.NhanSu;
namespace BatDongSan.Controllers.TinhCong
{
    public class ChamCongController : ApplicationController
    {

        private LinqNhanSuDataContext nhanSuContext = new LinqNhanSuDataContext();
        private IList<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan> phongBans;
        private NhanVienModel model;
        private StringBuilder buildTree;
        private readonly string MCV = "ChamCongAdmin";
        private bool? permission;
        public ActionResult XemTinhHinhRaVao()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            //thang(DateTime.Now.Month);
            //nam(DateTime.Now.Year);
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            //s var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            ViewBag.TuNgay = string.Format("{0:dd/MM/yyyy}", firstDayOfMonth);
            ViewBag.DenNgay = string.Format("{0:dd/MM/yyyy}", DateTime.Now);
            return View("");
        }
        public ActionResult LoadXemTinhHinhRaVao(string tuNgay, string denNgay, string qSearch, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!String.IsNullOrEmpty(tuNgay))
            {
                fromDate = String.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            if (!String.IsNullOrEmpty(denNgay))
            {
                toDate = String.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            string maNhanVien = GetUser().manv;
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;

            int total = nhanSuContext.sp_NS_XemTinhHinhRaVao_New(fromDate, toDate, maNhanVien, qSearch).Count();
            PagingLoaderController("/ChamCong/XemTinhHinhRaVao/", total, page, "?qsearch=" + qSearch + "&maNhanVien=" + maNhanVien);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_XemTinhHinhRaVao_New(fromDate, toDate, maNhanVien, qSearch).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadXemTinhHinhRaVao");

        }
        public ActionResult XemTinhHinhRaVaoCongNhan()
        {
            #region Role user
            permission = GetPermission("ChamCongAdminCN", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");
        }
        public ActionResult LoadXemTinhHinhRaVaoCongNhan(string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission("ChamCongAdminCN", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            string maNhanVien = GetUser().manv;
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;

            int total = nhanSuContext.sp_NS_XemTinhHinhRaVaoCongNhan(thang, nam, qSearch).Count();
            PagingLoaderController("/ChamCong/XemTinhHinhRaVaoCongNhan/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_XemTinhHinhRaVaoCongNhan(thang, nam, qSearch).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadXemTinhHinhRaVaoCongNhan");

        }
        public ActionResult XemBangLuongThang13()
        {
            #region Role user
            permission = GetPermission("XemBangLuongThang13", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion


            ViewBag.isGet = "True";
            nam(DateTime.Now.Year);
            return View();

        }
        public ActionResult LoadXemBangLuongThang13(int? page, string qSearch, int nam)
        {
            var userName = GetUser().manv;

            ViewBag.isGet = "True";
            var tblBangLuongThang13s = nhanSuContext.sp_NS_BangLuongThang13_Index(nam, qSearch).Where(d => d.maNhanVien == GetUser().manv).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = tblBangLuongThang13s.Count();
            ViewBag.Search = qSearch;
            ViewBag.nam = nam;
            ViewBag.maNhanVien = GetUser().manv;
            int checkDuyet = 0;
            var Duyets = nhanSuContext.tbl_DuyetLuongThang13s.Where(d => d.nam == nam).FirstOrDefault();
            if (Duyets != null) checkDuyet = 1;
            ViewBag.checkDuyet = checkDuyet;
            return PartialView("ViewXemBangLuongThang13", tblBangLuongThang13s.ToPagedList(currentPageIndex, 100));

        }
        public ActionResult XemBangLuongThanhTich()
        {
            #region Role user
            permission = GetPermission("BangLuongThanhTich", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion


            ViewBag.isGet = "True";
            nam(DateTime.Now.Year);
            return View();

        }
        public ActionResult LoadXemBangLuongThanhTich(int? page, int nam, int quy)
        {
            #region Role user
            permission = GetPermission("BangLuongThanhTich", BangPhanQuyen.QuyenXem);

            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var thongTin = nhanSuContext.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == GetUser().manv).ToList();

            var listTT = (from p in nhanSuContext.tbl_NS_TiLeThanhTich_DSNhanViens
                          join q in nhanSuContext.tbl_NS_TiLeThanhTiches on p.soPhieu equals q.soPhieu
                          where ((q.quy == quy || q.quy == (quy + 1) || q.quy == (quy + 2)) && q.nam == nam && q.xacNhan == true && p.maNhanVien == GetUser().manv)
                          select new TiLeThanhTich
                          {
                              nam = q.nam ?? 0,
                              thang = q.quy,
                              tyle = p.tyle ?? 0,
                              luongThanhTich = p.luongThanhTich,
                              tienThue = p.tienThue,
                              thucNhan = p.thucNhan
                          }).ToList();
            ViewBag.thongTin = thongTin;
            int currentPageIndex = page.HasValue ? page.Value : 1;
            return PartialView("ViewXemBangLuongThanhTich", listTT.ToPagedList(currentPageIndex, 100));

        }
        public ActionResult XemBangLuongThuongTet()
        {
            #region Role user
            permission = GetPermission("BangLuongThuongTet", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion


            ViewBag.isGet = "True";
            nam(DateTime.Now.Year);
            return View();

        }
        public ActionResult LoadXemBangLuongThuongTet(int? page, int nam, int quy)
        {
            #region Role user
            permission = GetPermission("BangLuongThuongTet", BangPhanQuyen.QuyenXem);

            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var tblBangLuongTT = nhanSuContext.sp_NS_ThanhTich_Tam(GetUser().manv, quy, nam).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;

            return PartialView("ViewXemBangLuongThuongTet", tblBangLuongTT.ToPagedList(currentPageIndex, 100));

        }
        public ActionResult XemBangLuong()
        {
            #region Role user
            permission = GetPermission("XemBangLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            nam(DateTime.Now.Year);
            return View("");
        }
        public ActionResult ViewChiTietLuong(string thang, string nam)
        {
            #region Role user
            permission = GetPermission("XemBangLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            var dsMauIn = nhanSuContext.GetTable<BatDongSan.Models.HeThong.Sys_PrintTemplate>().Where(d => d.maMauIn == "MIBLNV").FirstOrDefault();
            if (nam == "2018")
            {
                dsMauIn = nhanSuContext.GetTable<BatDongSan.Models.HeThong.Sys_PrintTemplate>().Where(d => d.maMauIn == "MIBLNV2018").FirstOrDefault();
            }
            string noiDung = string.Empty;
            var ds = nhanSuContext.tbl_NS_BangLuongNhanViens
                               .Where(t => t.maNhanVien == GetUser().manv && t.thang.ToString() == thang && nam == t.nam.ToString()).FirstOrDefault();
            var hinhThucLuong = nhanSuContext.tbl_NS_HinhThucLuongs.Where(d => d.maHinhThuc == ds.hinhThucLuong).Select(d => d.tenHinhThuc).FirstOrDefault() ?? String.Empty;
            ViewData["chiTiet"] = dsMauIn;
            if (dsMauIn != null)
            {
                var LuongTheoNgayCong = ds.luongThang ?? 0;
                var TongTienPhuCap = (ds.phuCapTienAn ?? 0) + (ds.phuCapDienThoai ?? 0) + (ds.phuCapThuHut ?? 0) + (Convert.ToDouble(ds.phuCapDiLaiNew ?? 0));
                var TongThuNhap = LuongTheoNgayCong + TongTienPhuCap + (ds.congTacPhi ?? 0) + (ds.sinhHoatPhi ?? 0);
                var TongKhauTru = (ds.baoHiemXH ?? 0) + (ds.baoHiemYTe ?? 0) + (ds.baoHiemTN ?? 0) + (ds.thue ?? 0) + (ds.doanPhi ?? 0);
                if (ds.phuCapDiLai == 2)
                {
                    var TruBaoHiem = (ds.baoHiemXH ?? 0) + (ds.baoHiemYTe ?? 0) + (ds.baoHiemTN ?? 0);
                    TongKhauTru = TongKhauTru - TruBaoHiem;
                }
                if (ds.phuCapTienXang == 2)
                {
                    TongKhauTru = TongKhauTru - (ds.thue ?? 0);
                }
                var TongPhuCapTL = (ds.truyLanh ?? 0) - (ds.truyThu ?? 0) - (ds.dangPhi ?? 0) + (ds.phuCapKhac ?? 0) + (ds.luongThanhTich1 ?? 0) + (ds.soNgayPhepTang ?? 0);
                var TongThucLanh = TongThuNhap - TongKhauTru + TongPhuCapTL;
                var soNgayTangCa = (ds.soNgayTangCaThuong ?? 0) + (ds.soNgayTangCaNgayLe ?? 0) + (ds.soNgayTangCaChuNhat ?? 0);
                noiDung = dsMauIn.html.Replace("{$thang}", Convert.ToString(thang))
                    .Replace("{$nam}", Convert.ToString(nam))
                    .Replace("{$hoVaTen}", ds.hoTen)
                    //capBacChucVu 
                    .Replace("{$capBacChucVu}", ds.chucVu)
                    .Replace("{$phongBan}", ds.phongBan)
                    .Replace("{$bac}", Convert.ToString(ds.bac))
                    .Replace("{$hinhThucLuong}", Convert.ToString(hinhThucLuong))
                    //A. Tổng Lương 
                    .Replace("{$tongLuong}", String.Format("{0:###,##0}", ds.tongLuong ?? 0))
                    .Replace("{$luongCoBan}", String.Format("{0:###,##0}", ds.phuCapLuong ?? 0))
                    .Replace("{$luongThanhTich}", String.Format("{0:###,##0}", ds.khoanBoSungLuong ?? 0))
                    .Replace("{$luongChoViec}", String.Format("{0:###,##0}", ds.luongChoViec ?? 0))
                    //B. Ngày Công
                    .Replace("{$ngayCongChuan}", Convert.ToString(ds.ngayCongChuan ?? 0))
                    .Replace("{$ngayCongTinhLuong}", Convert.ToString(ds.tongNgayCong ?? 0))
                    .Replace("{$ngayCong}", Convert.ToString(ds.soNgayQuet ?? 0))
                    .Replace("{$ngayCongTac}", Convert.ToString(ds.soNgayCongTac ?? 0))
                    .Replace("{$nghiPhep}", Convert.ToString(ds.soNgayNghiPhep ?? 0))
                    .Replace("{$nghiBu}", Convert.ToString(ds.soNgayNghiBu ?? 0))
                    .Replace("{$nghiLeTet}", Convert.ToString(ds.soNgayNghiLe ?? 0))
                    .Replace("{$ngayNghiKhongLuong}", Convert.ToString(ds.soNgayNghiKhongLuong ?? 0))
                    .Replace("{$soNgayTangCaThuong+soNgayTangCaLe+soNgayTangCaChuNhat}", Convert.ToString(soNgayTangCa))
                    //C. Lương Theo Ngày Công (A1*B2/B1)
                    .Replace("{$luongTheoNgayCong}", String.Format("{0:###,##0}", ds.luongThang ?? 0))
                    //Chờ Việc
                    //.Replace("{$luongChoViec}", String.Format("{0:###,##0}", ds.luongChoViec??0))
                    //.Replace("{$congChoViec}", Convert.ToString(ds.soNgayCongChoViec??0))
                    //.Replace("{$luongChoViec}", String.Format("{0:###,##0}", ds.luongChoViec??0))
                    // Các Khoản Phụ Cấp
                    .Replace("{$tongTienPhuCap}", String.Format("{0:###,##0}", TongTienPhuCap))
                    .Replace("{$tienAnGiuaCa}", String.Format("{0:###,##0}", ds.phuCapTienAn ?? 0))
                    .Replace("{$tienDienThoai}", String.Format("{0:###,##0}", ds.phuCapDienThoai ?? 0))
                    .Replace("{$tienThuHut}", String.Format("{0:###,##0}", ds.phuCapThuHut ?? 0))
                    .Replace("{$phuCapDiLai}", String.Format("{0:###,##0}", ds.phuCapDiLaiNew ?? 0))
                    .Replace("{$congTacPhi}", String.Format("{0:###,##0}", ds.congTacPhi ?? 0))
                    .Replace("{$sinhHoatPhi}", String.Format("{0:###,##0}", ds.sinhHoatPhi ?? 0))
                    .Replace("{$dangPhi}", String.Format("{0:###,##0}", ds.dangPhi ?? 0))
                    .Replace("{$soNgayCongChoViec}", String.Format("{0:###,##0}", ds.soNgayCongChoViec ?? 0))
                    //tongThuNhap=   Lương Theo Ngày Công (A1*B2/B1) +  Các Khoản Phụ Cấp
                     .Replace("{$tongThuNhap}", String.Format("{0:###,##0}", TongThuNhap))
                    //tongThucLanh=  Tổng Thu Nhập(C+D) 
                     .Replace("{$tongThucLanh}", String.Format("{0:###,##0}", TongThucLanh))
                    // Các Khoản Khấu Trừ

                    .Replace("{$tongKhauTru}", String.Format("{0:###,##0}", TongKhauTru))
                    .Replace("{$tienBHXH}", String.Format("{0:###,##0}", ds.baoHiemXH ?? 0))
                    .Replace("{$tienBHYT}", String.Format("{0:###,##0}", ds.baoHiemYTe ?? 0))
                    .Replace("{$tienBHTN}", String.Format("{0:###,##0}", ds.baoHiemTN ?? 0))
                    .Replace("{$tienTTNCT}", String.Format("{0:###,##0}", ds.thue ?? 0))
                    .Replace("{$tienDoanPhi}", String.Format("{0:###,##0}", ds.doanPhi ?? 0))
                    //Phụ Cấp Khác Và Truy Lĩnh
                    .Replace("{$tongPhuCapTL}", String.Format("{0:###,##0}", TongPhuCapTL))
                    .Replace("{$tienPhuCap}", String.Format("{0:###,##0}", ds.phuCapKhac ?? 0))
                    .Replace("{$tienTTTLThue}", String.Format("{0:###,##0}", ds.soNgayPhepTang ?? 0))
                    .Replace("{$tienTruyThu}", String.Format("{0:###,##0}", ds.truyThu ?? 0))
                    .Replace("{$tienTruyLanh}", String.Format("{0:###,##0}", ds.truyLanh ?? 0))
                    .Replace("{$luongtt1}", String.Format("{0:###,##0}", ds.luongThanhTich1 ?? 0))
                    //Tạm Ứng
                     .Replace("{$tamUng}", String.Format("{0:###,##0}", ds.tamUng ?? 0))
                    //Thực Nhận Lương Tháng (C+D+E-F+G-H)
                    .Replace("{$tongThucNhanLuong}", String.Format("{0:###,##0}", ds.thucLanh ?? 0))
                    .Replace("{$quyetToanLuongT1}", String.Format("{0:###,##0}", ds.luongThanhTich2 ?? 0))
                .Replace("{$truyThuThueThuongTTT13_2016}", String.Format("{0:###,##0}", ds.luongThanhTich3 ?? 0))
                .Replace("{$luongThanhTich3}", String.Format("{0:###,##0}", ds.luongThanhTich3 ?? 0))
                .Replace("{$soNgayPhepTang}", String.Format("{0:###,##0}", ds.soNgayPhepTang ?? 0)); ;

            }
            ViewBag.NoiDung = noiDung;
            // return PartialView("_ViewChiTietLuong");
            return PartialView("_ViewChiTietLuongTemplate");
        }
        public ActionResult LoadXemBangLuong(int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission("XemBangLuong", BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            string maNhanVien = GetUser().manv;
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangLuongDanhChoNhanVien(maNhanVien, nam).Count();
            PagingLoaderController("/ChamCong/XemBangLuong/", total, page, "?maNhanVien=" + maNhanVien);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangLuongDanhChoNhanVien(maNhanVien, nam).Skip(start).Take(offset).ToList();

            return PartialView("_LoadXemBangLuong");
        }
        public ActionResult XemBangChamCongChiTiet()
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            buildTree = new StringBuilder();
            phongBans = nhanSuContext.GetTable<BatDongSan.Models.DanhMuc.tbl_DM_PhongBan>().ToList();
            buildTree = TreePhongBanAjax.BuildTreeDepartment(phongBans);
            ViewBag.PhongBans = buildTree.ToString();
            thang(DateTime.Now.Month);
            nam(DateTime.Now.Year);
            return View("");
        }
        public ActionResult LoadBangChamCongChiTiet(string qSearch, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            string maNhanVien = GetUser().manv;
            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            int total = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, maNhanVien).Count();
            PagingLoaderController("/ChamCong/LoadBangChamCongChiTiet/", total, page, "?qsearch=" + qSearch);
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangChamCongChiTiet(thang, nam, qSearch, maNhanVien).Skip(start).Take(offset).ToList();

            ViewData["qSearch"] = qSearch;
            return PartialView("_LoadBangChamCongChiTiet");
        }





        public ActionResult LoadBangChamCongTongHop(string qSearch, string maPhongBan, int thang, int nam, int _page = 0)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            int page = _page == 0 ? 1 : _page;
            int pIndex = page;
            string maNhanVien = GetUser().manv;
            // Check if chua duyet thi tra lai

            var checkEx = nhanSuContext.tbl_DuyetBangTongHopCongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
            if (checkEx != null)
            {
                //End check
                int total = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, maNhanVien, 0, maPhongBan).Count();
                PagingLoaderController("/BangChamCongTongHop/LoadBangChamCongTongHop/", total, page, "?qsearch=" + qSearch);
                ViewData["lsDanhSach"] = nhanSuContext.sp_NS_BangTongHopCongThang(thang, nam, qSearch, maNhanVien, 0, maPhongBan).Skip(start).Take(offset).ToList();

                ViewData["qSearch"] = qSearch;
            }

            return PartialView("_LoadBangChamCongTongHop");
        }
        public ActionResult CheckDuyetTongHopCong(int thang, int nam)
        {


            try
            {


                var checkEx = nhanSuContext.tbl_DuyetBangTongHopCongNVs.Where(t => t.nam == nam && t.thang == thang).FirstOrDefault();
                if (checkEx != null)
                {
                    var result = new { kq = true };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var result = new { kq = false };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                //End check
            }
            catch
            {
                var result = new { kq = false };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        private void thang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 0; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["thang"] = new SelectList(dics, "Key", "Value", value);
            ViewData["thangtc"] = new SelectList(dics, "Key", "Value", value);
        }
        private void nam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["nam"] = new SelectList(dics, "Key", "Value", value);
            ViewData["namtc"] = new SelectList(dics, "Key", "Value", value);
        }

        public ActionResult LoadXemLichSuRaVao(string maPhieu)
        {
            ViewData["lsDanhSach"] = nhanSuContext.sp_NS_XemLichSuRaVao(maPhieu).ToList();
            return PartialView("_LoadXemLichSuRaVao");
        }
    }
}
