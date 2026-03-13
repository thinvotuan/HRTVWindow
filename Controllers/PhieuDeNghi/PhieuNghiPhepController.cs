using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Helper.Utils;
using BatDongSan.Models.NhanSu;
using BatDongSan.Models.PhieuDeNghi;
using BatDongSan.Models.HeThong;
using BatDongSan.Models.DanhMuc;
using System.Text;
using BatDongSan.Utils.Paging;
using BatDongSan.Helper.Common;
using System.Globalization;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel.Contrib;
using NPOI.SS.Util;
using Worldsoft.Mvc.Web.Util;
using System.Text.RegularExpressions;

namespace BatDongSan.Controllers.PhieuDeNghi
{
    public class PhieuNghiPhepController : ApplicationController
    {
        LinqPhieuDeNghiDataContext lqPhieuDN = new LinqPhieuDeNghiDataContext();
        LinqNhanSuDataContext linqNS = new LinqNhanSuDataContext();
        LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
        tbl_NS_PhieuNghiPhep tblPhieuDeNghi;
        IList<tbl_NS_PhieuNghiPhep> tblPhieuDeNghis;
        PhieuDeNghiNghiPhep PhieuDeNghiModel;
        public const string taskIDSystem = "PhieuNghiPhep";//REQUESTLEAVE
        public const string taskIDSystemReceiver = "RECEIVERREQUESTLEAVE";
        public bool? permission;
        //
        // GET: /PhieuNghiPhep/

        public ActionResult Index(int? page, string searchString, string tuNgay, string denNgay, string trangThai, string loaiNghi)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                ViewBag.QuyenDuyet = GetPermission(taskIDSystem, BangPhanQuyen.QuyenDuyet);
                Administrator(GetUser().manv);
                AdminNhanSu(GetUser().manv);
                var userName = GetUser().manv;
                BindDataTrangThai(taskIDSystem);
                BindDataLeave_Index(loaiNghi);
                DateTime? fromDate = null;
                DateTime? toDate = null;
                if (!String.IsNullOrEmpty(tuNgay))
                    fromDate = DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(denNgay))
                    toDate = DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                ViewBag.isGet = "True";
                var tblPhieuDeNghis = lqPhieuDN.sp_NS_PhieuDangKyNghiPhep_Index(trangThai, loaiNghi, fromDate, toDate, searchString, userName).ToList();
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Count = tblPhieuDeNghis.Count();
                ViewBag.Search = searchString;
                ViewBag.tuNgay = tuNgay;
                ViewBag.denNgay = tuNgay;
                ViewBag.trangThai = trangThai;
                ViewBag.loaiNghi = loaiNghi;
                return View(tblPhieuDeNghis.ToPagedList(currentPageIndex, 50));
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }
        public ActionResult ViewIndex(int? page, string searchString, string tuNgay, string denNgay, string trangThai, string loaiNghi)
        {
            try
            {
                ViewBag.QuyenDuyet = GetPermission(taskIDSystem, BangPhanQuyen.QuyenDuyet);
                Administrator(GetUser().manv);
                var userName = GetUser().manv;
                BindDataLeave_Index(loaiNghi);
                DateTime? fromDate = null;
                DateTime? toDate = null;
                if (!String.IsNullOrEmpty(tuNgay))
                    fromDate = DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(denNgay))
                    toDate = DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                ViewBag.isGet = "True";
                var tblPhieuDeNghis = lqPhieuDN.sp_NS_PhieuDangKyNghiPhep_Index(trangThai, loaiNghi, fromDate, toDate, searchString, userName).ToList();
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Count = tblPhieuDeNghis.Count();
                ViewBag.Search = searchString;
                ViewBag.tuNgay = tuNgay;
                ViewBag.denNgay = tuNgay;
                ViewBag.trangThai = trangThai;
                ViewBag.loaiNghi = loaiNghi;
                return PartialView("ViewIndex", tblPhieuDeNghis.ToPagedList(currentPageIndex, 50));
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }

        #region Binddata

        private void BindDataLeave_Index(string leaveCode)
        {

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("", "[Chọn]");

            foreach (var item in linqDM.tbl_DM_LoaiNghiPheps.ToList())
            {
                dict.Add(item.maLoaiNghiPhep, item.tenLoaiNghiPhep);
            }
            ViewBag.leaveTypes = new SelectList(dict, "Key", "Value", leaveCode);
        }

        #endregion
        public ActionResult Create()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                PhieuDeNghiModel = new PhieuDeNghiNghiPhep();
                PhieuDeNghiModel.maPhieu = GenerateUtil.CheckLetter("DKNP", GetMax());
                PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
                {
                    maNhanVien = GetUser().manv,
                    hoVaTen = HoVaTen(GetUser().manv),
                    tenPhongBan = linqDM.sp_PB_DanhSachNhanVien(GetUser().manv, null).Select(d => d.phongBan).FirstOrDefault(),
                    tenChucDanh = linqDM.sp_PB_DanhSachNhanVien(GetUser().manv, null).Select(d => d.chucDanh).FirstOrDefault(),
                    tenChiNhanh = GetChiNhanh(GetUser().manv)
                };
                PhieuDeNghiModel.soNgayNghiPhep = 1;
                PhieuDeNghiModel.phanTramTinhHuong = 100;
                PhieuDeNghiModel.soNgayTangCa = 0;
                BindDataLeave(null, GetUser().manv);
                GetHaftDayStart("1");
                GetHaftDayEnd("1");
                var phieu = lqPhieuDN.tbl_NS_PhieuNghiPheps.Where(d => d.maNhanVien == GetUser().manv).OrderByDescending(x => x.maPhieu).FirstOrDefault();
                ViewBag.ngayGioiHan = phieu == null ? "" : (phieu.ngayBatDau == null ? string.Format("{0:dd/MM/yyyy}", phieu.ngayBatDau) : string.Format("{0:dd/MM/yyyy}", phieu.ngayKetThuc));
                //Kiểm tra nhân viên đó có được quyền tạo phiếu trực tiếp hay không (Admin nhân sự mới được quyền tạo phiếu trực tiếp)
                string flag = "false";
                if (lqPhieuDN.fnc_HTQuyenXemPhieu(GetUser().manv, taskIDSystem, string.Empty).Count() > 0)
                {
                    flag = "true";
                }
                //AdminNhanSu(GetUser().manv);
                if (flag == "true")
                {
                    PhieuDeNghiModel.NhanVienLapPhieuTT = new NhanVienModel
                    {
                        maNhanVien = GetUser().manv,
                        hoVaTen = HoVaTen(GetUser().manv)
                    };
                }
                ViewBag.AdminNhanSu = flag;
                return View(PhieuDeNghiModel);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection coll)
        {
            try
            {
                // Check lai loai nghi phep
                //string flag = AdminNhanSu(GetUser().manv);
                //string flag = "false";
                //if (lqPhieuDN.fnc_HTQuyenXemPhieu(GetUser().manv, taskIDSystem, string.Empty).Count() > 0)
                //{
                //    flag = "true";
                //}
                tblPhieuDeNghi = new tbl_NS_PhieuNghiPhep();

                tblPhieuDeNghi.maNhanVien = coll.Get("NhanVien.maNhanVien");
                if (tblPhieuDeNghi.maNhanVien == null || tblPhieuDeNghi.maNhanVien == "")
                {
                    return View("error");
                }
                int giatriCheck = CheckLoaiNghiPhep(coll.Get("ngayBatDau"), tblPhieuDeNghi.maNhanVien, Convert.ToInt32(coll.Get("loaiNghiBatDau")), coll.Get("loaiNghiPhep"));
                if (giatriCheck == 1)
                {
                    return View("error");
                }
                else
                {

                    tblPhieuDeNghi.nguoiLap = GetUser().manv;
                    tblPhieuDeNghi.soNgayNamVien = 0;
                    tblPhieuDeNghi.loaiThoiGianNghi = Convert.ToBoolean(coll.Get("thoiGianNghi"));

                    tblPhieuDeNghi.ngayBatDau = String.IsNullOrEmpty(coll.Get("ngayBatDau")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayBatDau"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    tblPhieuDeNghi.loaiNgayBatDau = Convert.ToInt32(coll.Get("loaiNghiBatDau"));

                    tblPhieuDeNghi.ngayKetThuc = String.IsNullOrEmpty(coll.Get("ngayKetThuc")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayKetThuc"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    tblPhieuDeNghi.loaiNgayKetThuc = Convert.ToInt32(coll.Get("loaiNghiKetThuc"));

                    tblPhieuDeNghi.loaiNghiPhep = coll.Get("loaiNghiPhep");
                    tblPhieuDeNghi.lyDo = coll.Get("lyDo");

                    tblPhieuDeNghi.phanTramHuongNamVien = 0;
                    tblPhieuDeNghi.ngaylap = DateTime.Now;
                    if (coll.Get("const_SoNgayNghi") != coll.Get("soNgayNghiPhep"))
                    {
                        return View("error");
                    }
                    // Tinh lai so ngay nghi phep
                    var manv = tblPhieuDeNghi.maNhanVien;
                    string fromDate = coll.Get("ngayBatDau");
                    string toDate = coll.Get("ngayKetThuc");

                    double soNgayBatDau = !string.IsNullOrEmpty(coll.Get("loaiNghiBatDau")) ? Convert.ToDouble(coll.Get("loaiNghiBatDau")) : 0;
                    double soNgayKetThuc = !String.IsNullOrEmpty(coll.Get("loaiNghiKetThuc")) ? Convert.ToDouble(coll.Get("loaiNghiKetThuc")) : 0;
                    if (soNgayBatDau == 2 || soNgayBatDau == 3) { soNgayBatDau = 0.5; } else { soNgayBatDau = 1; }
                    if (soNgayKetThuc == 2 || soNgayKetThuc == 3) { soNgayKetThuc = 0.5; } else { soNgayKetThuc = 1; }

                    double kq = CountNgayNghiPhep(manv, fromDate, toDate, soNgayBatDau, soNgayKetThuc);
                    if (kq == 0)
                    {
                        return View("error");
                    }
                    //// Check so ngay nghi bu
                    if (tblPhieuDeNghi.loaiNghiPhep == "OTHERLEAVE")
                    {
                        double tongNgayDuocNghi = TinhSoNgayTangCaEdit(tblPhieuDeNghi.maNhanVien);
                        if (tongNgayDuocNghi < kq)
                        {
                            return View("error");
                        }

                    }
                    //// End check so ngay nghi bu
                    tblPhieuDeNghi.soNgayNghiPhepThucTe = Convert.ToDecimal(kq);

                    //End

                    // Tinh phan tram tinh luong

                    tblPhieuDeNghi.phanTramHuongLuong = Convert.ToDecimal(coll.Get("phanTramTinhHuong"));
                    // Check so ngay phep nam con lai
                    if (tblPhieuDeNghi.loaiNghiPhep == "YEARLEAVE")
                    {
                        double soNgayPhepNamCL = CountDayPhepNamControl(tblPhieuDeNghi.maNhanVien);
                        if (soNgayPhepNamCL < kq)
                        {
                            return View("error");

                        }
                    }
                    tblPhieuDeNghi.maPhieu = GenerateUtil.CheckLetter("DKNP", GetMax());
                    lqPhieuDN.tbl_NS_PhieuNghiPheps.InsertOnSubmit(tblPhieuDeNghi);
                    lqPhieuDN.SubmitChanges();

                    return RedirectToAction("Edit", new { id = tblPhieuDeNghi.maPhieu });
                }
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        public ActionResult Edit(string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                ViewBag.QuyenDuyet = GetPermission(taskIDSystem, BangPhanQuyen.QuyenDuyet);
                Administrator(GetUser().manv);
                var dataPhieuCongTac = lqPhieuDN.tbl_NS_PhieuNghiPheps.Where(d => d.maPhieu == id).FirstOrDefault();
                PhieuDeNghiModel = new PhieuDeNghiNghiPhep();
                PhieuDeNghiModel.maPhieu = dataPhieuCongTac.maPhieu;
                PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
                PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
                {
                    maNhanVien = dataPhieuCongTac.maNhanVien,
                    hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien),
                    tenPhongBan = linqDM.sp_PB_DanhSachNhanVien(dataPhieuCongTac.maNhanVien, null).Select(d => d.phongBan).FirstOrDefault(),
                    tenChucDanh = linqDM.sp_PB_DanhSachNhanVien(dataPhieuCongTac.maNhanVien, null).Select(d => d.chucDanh).FirstOrDefault(),
                    tenChiNhanh = GetChiNhanh(dataPhieuCongTac.maNhanVien)
                };

                PhieuDeNghiModel.NhanVienLapPhieuTT = new BatDongSan.Models.NhanSu.NhanVienModel
                {
                    maNhanVien = dataPhieuCongTac.nguoiLap,
                    hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap),
                };

                PhieuDeNghiModel.thoiGianNghi = dataPhieuCongTac.loaiThoiGianNghi;
                PhieuDeNghiModel.soNgayNghiPhep = dataPhieuCongTac.soNgayNghiPhepThucTe;
                PhieuDeNghiModel.phanTramTinhHuong = dataPhieuCongTac.phanTramHuongLuong;


                PhieuDeNghiModel.ngayBatDau = dataPhieuCongTac.ngayBatDau;
                PhieuDeNghiModel.ngayKetThuc = dataPhieuCongTac.ngayKetThuc;
                PhieuDeNghiModel.lyDo = dataPhieuCongTac.lyDo;
                PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
                DMNguoiDuyetController nd = new DMNguoiDuyetController();
                PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(PhieuDeNghiModel.maPhieu, PhieuDeNghiModel.maQuiTrinhDuyet);
                BindDataLeave(dataPhieuCongTac.loaiNghiPhep, dataPhieuCongTac.maNhanVien);
                GetHaftDayStart(dataPhieuCongTac.loaiNgayBatDau.ToString());
                GetHaftDayEnd(dataPhieuCongTac.loaiNgayKetThuc.ToString());
                var phieu = lqPhieuDN.tbl_NS_PhieuNghiPheps.Where(d => d.maNhanVien == dataPhieuCongTac.maNhanVien).OrderByDescending(x => x.maPhieu).FirstOrDefault();
                ViewBag.ngayGioiHan = phieu == null ? "" : (phieu.ngayBatDau == null ? string.Format("{0:dd/MM/yyyy}", phieu.ngayBatDau) : string.Format("{0:dd/MM/yyyy}", phieu.ngayKetThuc));
                LinqHeThongDataContext ht = new LinqHeThongDataContext();
                string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
                ViewBag.HoTen = hoTen;
                int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
                ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
                ViewBag.URL = Request.Url.AbsoluteUri.ToString();
                ViewBag.LoaiBatDauNghi = dataPhieuCongTac.loaiNgayBatDau.ToString();
                ViewBag.LoaiKetThucNghi = dataPhieuCongTac.loaiNgayKetThuc.ToString();
                // Get so ngay rang buoc
                var soNgayRangBuoc = 0;
                var dataNRB = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == dataPhieuCongTac.loaiNghiPhep).FirstOrDefault();
                if (dataNRB != null)
                {
                    soNgayRangBuoc = dataNRB.soNgayRangBuoc != null ? Convert.ToInt32(dataNRB.soNgayRangBuoc) : 0;
                }

                PhieuDeNghiModel.soNgayRangBuoc = soNgayRangBuoc;
                //Get So ngay con lai nghi bu
                double soNgayTangCa = 0;
                if (dataPhieuCongTac.loaiNghiPhep == "OTHERLEAVE")
                {
                    soNgayTangCa = TinhSoNgayTangCaEdit(dataPhieuCongTac.maNhanVien);
                }
                PhieuDeNghiModel.soNgayTangCa = Convert.ToDecimal(soNgayTangCa);
                return View(PhieuDeNghiModel);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(string id, FormCollection coll)
        {
            try
            {
                // string flag = AdminNhanSu(GetUser().manv);
                //string flag = "false";
                //if (lqPhieuDN.fnc_HTQuyenXemPhieu(GetUser().manv, taskIDSystem, string.Empty).Count() > 0)
                //{
                //    flag = "true";
                //}
                int giatriCheck = CheckLoaiNghiPhep(coll.Get("ngayBatDau"), GetUser().manv, Convert.ToInt32(coll.Get("loaiNghiBatDau")), coll.Get("loaiNghiPhep"));
                if (giatriCheck == 1)
                {
                    return View("error");
                }
                else
                {
                    tblPhieuDeNghi = lqPhieuDN.tbl_NS_PhieuNghiPheps.Where(d => d.maPhieu == id).FirstOrDefault();

                    tblPhieuDeNghi.loaiThoiGianNghi = Convert.ToBoolean(coll.Get("thoiGianNghi"));
                    tblPhieuDeNghi.ngayBatDau = String.IsNullOrEmpty(coll.Get("ngayBatDau")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayBatDau"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    tblPhieuDeNghi.loaiNgayBatDau = Convert.ToInt32(coll.Get("loaiNghiBatDau"));

                    tblPhieuDeNghi.ngayKetThuc = String.IsNullOrEmpty(coll.Get("ngayKetThuc")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayKetThuc"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    tblPhieuDeNghi.loaiNgayKetThuc = Convert.ToInt32(coll.Get("loaiNghiKetThuc"));

                    tblPhieuDeNghi.loaiNghiPhep = coll.Get("loaiNghiPhep");
                    tblPhieuDeNghi.lyDo = coll.Get("lyDo");
                    if (coll.Get("const_SoNgayNghi") != coll.Get("soNgayNghiPhep"))
                    {
                        return View("error");
                    }
                    // Tinh lai so ngay nghi phep
                    var manv = tblPhieuDeNghi.maNhanVien;
                    string fromDate = coll.Get("ngayBatDau");
                    string toDate = coll.Get("ngayKetThuc");

                    double soNgayBatDau = !string.IsNullOrEmpty(coll.Get("loaiNghiBatDau")) ? Convert.ToDouble(coll.Get("loaiNghiBatDau")) : 0;
                    double soNgayKetThuc = !String.IsNullOrEmpty(coll.Get("loaiNghiKetThuc")) ? Convert.ToDouble(coll.Get("loaiNghiKetThuc")) : 0;
                    if (soNgayBatDau == 2 || soNgayBatDau == 3) { soNgayBatDau = 0.5; } else { soNgayBatDau = 1; }
                    if (soNgayKetThuc == 2 || soNgayKetThuc == 3) { soNgayKetThuc = 0.5; } else { soNgayKetThuc = 1; }

                    double kq = CountNgayNghiPhep(manv, fromDate, toDate, soNgayBatDau, soNgayKetThuc);
                    if (kq == 0)
                    {
                        return View("error");
                    }
                    tblPhieuDeNghi.soNgayNghiPhepThucTe = Convert.ToDecimal(kq);

                    //End 


                    tblPhieuDeNghi.phanTramHuongLuong = Convert.ToDecimal(coll.Get("phanTramTinhHuong"));
                    //// Check so ngay nghi bu
                    if (tblPhieuDeNghi.loaiNghiPhep == "OTHERLEAVE")
                    {
                        double tongNgayDuocNghi = TinhSoNgayTangCaEdit(tblPhieuDeNghi.maNhanVien);
                        if (tongNgayDuocNghi < kq)
                        {
                            return View("error");
                        }

                    }

                    //// End check so ngay nghi bu
                    // Check so ngay phep nam con lai
                    if (tblPhieuDeNghi.loaiNghiPhep == "YEARLEAVE")
                    {
                        double soNgayPhepNamCL = CountDayPhepNamControl(tblPhieuDeNghi.maNhanVien);
                        if (soNgayPhepNamCL < kq)
                        {
                            return View("error");

                        }
                    }
                    LuuLichSuCapNhatPhieu(id, taskIDSystem, 0);
                    lqPhieuDN.SubmitChanges();

                    return RedirectToAction("Edit", new { id = tblPhieuDeNghi.maPhieu });
                }

            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXoa);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                var phieu = lqPhieuDN.tbl_NS_PhieuNghiPheps.Where(d => d.maPhieu == id).FirstOrDefault();
                lqPhieuDN.tbl_NS_PhieuNghiPheps.DeleteOnSubmit(phieu);
                LinqHeThongDataContext lqHT = new LinqHeThongDataContext();
                var nguoiDuyet = lqHT.tbl_HT_DMNguoiDuyets.Where(d => d.maPhieu == id).ToList();
                if (nguoiDuyet != null && nguoiDuyet.Count > 0)
                {
                    lqHT.tbl_HT_DMNguoiDuyets.DeleteAllOnSubmit(nguoiDuyet);
                }
                LuuLichSuCapNhatPhieu(id, taskIDSystem, 1);
                lqPhieuDN.SubmitChanges();
                lqHT.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        public ActionResult Details(string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXemChiTiet);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                var dataPhieuCongTac = lqPhieuDN.tbl_NS_PhieuNghiPheps.Where(d => d.maPhieu == id).FirstOrDefault();
                PhieuDeNghiModel = new PhieuDeNghiNghiPhep();
                PhieuDeNghiModel.maPhieu = dataPhieuCongTac.maPhieu;
                PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
                PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
                {
                    maNhanVien = dataPhieuCongTac.maNhanVien,
                    hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien),
                    tenPhongBan = linqDM.sp_PB_DanhSachNhanVien(dataPhieuCongTac.maNhanVien, null).Select(d => d.phongBan).FirstOrDefault(),
                    tenChucDanh = linqDM.sp_PB_DanhSachNhanVien(dataPhieuCongTac.maNhanVien, null).Select(d => d.chucDanh).FirstOrDefault(),
                    tenChiNhanh = GetChiNhanh(dataPhieuCongTac.maNhanVien)
                };

                PhieuDeNghiModel.NhanVienLapPhieuTT = new BatDongSan.Models.NhanSu.NhanVienModel
                {
                    maNhanVien = dataPhieuCongTac.nguoiLap,
                    hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap),
                };

                PhieuDeNghiModel.thoiGianNghi = dataPhieuCongTac.loaiThoiGianNghi;
                PhieuDeNghiModel.soNgayNghiPhep = dataPhieuCongTac.soNgayNghiPhepThucTe;
                PhieuDeNghiModel.phanTramTinhHuong = dataPhieuCongTac.phanTramHuongLuong;
                PhieuDeNghiModel.soNgayTangCa = 0;

                PhieuDeNghiModel.ngayBatDau = dataPhieuCongTac.ngayBatDau;
                PhieuDeNghiModel.ngayKetThuc = dataPhieuCongTac.ngayKetThuc;
                PhieuDeNghiModel.lyDo = dataPhieuCongTac.lyDo;
                PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
                DMNguoiDuyetController nd = new DMNguoiDuyetController();
                PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(PhieuDeNghiModel.maPhieu, PhieuDeNghiModel.maQuiTrinhDuyet);
                BindDataLeave(dataPhieuCongTac.loaiNghiPhep, dataPhieuCongTac.maNhanVien);
                GetHaftDayStart(dataPhieuCongTac.loaiNgayBatDau.ToString());
                GetHaftDayEnd(dataPhieuCongTac.loaiNgayKetThuc.ToString());
                var phieu = lqPhieuDN.tbl_NS_PhieuNghiPheps.Where(d => d.maNhanVien == dataPhieuCongTac.maNhanVien).OrderByDescending(x => x.maPhieu).FirstOrDefault();
                ViewBag.ngayGioiHan = phieu == null ? "" : (phieu.ngayBatDau == null ? string.Format("{0:dd/MM/yyyy}", phieu.ngayBatDau) : string.Format("{0:dd/MM/yyyy}", phieu.ngayKetThuc));
                LinqHeThongDataContext ht = new LinqHeThongDataContext();
                string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
                ViewBag.HoTen = hoTen;
                int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
                ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
                ViewBag.URL = Request.Url.AbsoluteUri.ToString();
                ViewBag.LoaiBatDauNghi = dataPhieuCongTac.loaiNgayBatDau.ToString();
                ViewBag.LoaiKetThucNghi = dataPhieuCongTac.loaiNgayKetThuc.ToString();
                //Tất cả Nhân viên phòng nhân sự duyệt
                //Get So ngay con lai nghi bu
                double soNgayTangCa = 0;
                if (dataPhieuCongTac.loaiNghiPhep == "OTHERLEAVE")
                {
                    soNgayTangCa = TinhSoNgayTangCaEdit(dataPhieuCongTac.maNhanVien);
                }
                PhieuDeNghiModel.soNgayTangCa = Convert.ToDecimal(soNgayTangCa);
                NhanVienQLNSDuyet();
                return View(PhieuDeNghiModel);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        public JsonResult GetLoaiNghiPhep(string maLoai)
        {
            try
            {
                tbl_DM_LoaiNghiPhep hrLeave = new tbl_DM_LoaiNghiPhep();
                hrLeave = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == maLoai).FirstOrDefault();
                var result = new { phanTramLuongHuong = hrLeave.phanTramLuongHuong, soNgayRangBuoc = hrLeave.soNgayRangBuoc };
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(null);
            }
        }
        public JsonResult GetSoNgayTangCa(string maNhanVien)
        {
            try
            {

                var maNV = maNhanVien;
                string qSearch = "";
                var dataSN = lqPhieuDN.sp_NS_TongSoGioTangCa(qSearch).Where(d => d.maNhanVien == maNhanVien).FirstOrDefault();
                if (dataSN != null)
                {
                    var result = new { soGioConLai = dataSN.soGioConLai, soNgayNghiBu = dataSN.soNgayNghiBu, soGioTangCa = dataSN.soGioTangCa };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = new { soGioConLai = 0, soNgayNghiBu = 0, soGioTangCa = 0 };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(null);
            }
        }
        public int CheckValidNgayTangCa(double soNgayNghiPhep, string maNhanVien)
        {
            var maNV = maNhanVien;
            string qSearch = "";
            var dataSN = lqPhieuDN.sp_NS_TongSoGioTangCa(qSearch).Where(d => d.maNhanVien == maNhanVien).FirstOrDefault();
            if (dataSN != null)
            {

                if (soNgayNghiPhep > Convert.ToDouble(dataSN.soGioConLai))
                {
                    return 0;
                }
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public double TinhSoNgayTangCaEdit(string maNhanVien)
        {

            string qSearch = "";
            var dataSN = lqPhieuDN.sp_NS_TongSoGioTangCa(qSearch).Where(d => d.maNhanVien == maNhanVien).FirstOrDefault();
            if (dataSN != null)
            {


                return Convert.ToDouble(dataSN.soGioConLai);

            }
            else
            {
                return 0;
            }
        }

        public decimal phanTramTL(string maLoai)
        {

            tbl_DM_LoaiNghiPhep hrLeave = new tbl_DM_LoaiNghiPhep();
            hrLeave = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == maLoai).FirstOrDefault();
            return Convert.ToDecimal(hrLeave.phanTramLuongHuong);


        }
        // Return 0 la ngay hop le
        public int CheckLoaiNghiPhep(string ngayBatDau, string maNhanVien, int? loaiNghi, string loaiNghiPhep)
        {
            string flag = "false";
            if (lqPhieuDN.fnc_HTQuyenXemPhieu(GetUser().manv, taskIDSystem, string.Empty).Count() > 0)
            {
                flag = "true";
            }
            DateTime fromDate = DateTime.ParseExact(ngayBatDau, "dd/MM/yyyy", null);
            DateTime ngayLap = DateTime.Now;
            var ls = lqPhieuDN.tbl_NS_PhieuNghiPheps.Where(t => t.maNhanVien.Equals(maNhanVien) && (t.ngayBatDau == fromDate ||
                (t.ngayBatDau < fromDate && t.ngayKetThuc != null && fromDate <= t.ngayKetThuc))).ToList();
            var getLoaiNghi = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == loaiNghiPhep).FirstOrDefault();
            if (getLoaiNghi == null)
            {
                return 1;
            }
            DateTime today = DateTime.Now;
            DateTime answer = today.AddDays(Convert.ToInt32(getLoaiNghi.soNgayRangBuoc));
            // ngay answer phai lon hon hoac bang ngay bat dau
            int result = DateTime.Compare(fromDate.Date, answer.Date);
            if (result == -1 && flag == "false")
            {
                return 1;
            }
            return 0;

        }

        #region Kiểm tra phiếu nghỉ phép đã tồn tại phiếu lập công tác hay chưa
        public ActionResult CheckPhieuCongTac(string loaiThoiGian, string ngayBatDau, string ngayKetThuc, string loaiNghiBatDau, string loaiNghiKetThuc, string maNhanVien)
        {
            try
            {
                return Json(1);
                DateTime fromDate = DateTime.ParseExact(ngayBatDau, "dd/MM/yyyy", null);
                if (loaiThoiGian == "False")//Trường hợp thời gian nghỉ trong ngày
                {
                    if ((lqPhieuDN.tbl_NS_PhieuCongTacs.Where(d => d.maNhanVien == maNhanVien).Count() == 0))//Nhân viên chưa lập phiếu công tác nào hết
                    {
                        return Json(1);
                    }
                    else//Nhân viên đã lập phiếu công tác
                    {
                        int count = lqPhieuDN.tbl_NS_PhieuCongTacs.Where(t => t.maNhanVien == maNhanVien && (t.ngayBatDau == fromDate || (t.ngayBatDau < fromDate && t.ngayKetThuc != null && fromDate <= t.ngayKetThuc))).Count();
                        if (count == 0)
                        {
                            return Json(1);
                        }
                        else
                        {
                            return Json(0);
                        }
                    }
                }
                else if (loaiThoiGian == "True")// Trường hợp thời gian nghỉ nhiều ngày
                {
                    if ((lqPhieuDN.tbl_NS_PhieuCongTacs.Where(d => d.maNhanVien == maNhanVien).Count() == 0))//Nhân viên chưa lập phiếu công tác nào hết
                    {
                        return Json(1);
                    }
                    else//Nhân viên đã lập phiếu công tác
                    {
                        //Kiểm tra ngày kết thúc
                        if (!String.IsNullOrEmpty(ngayKetThuc))
                        {
                            DateTime fromDateKT = DateTime.ParseExact(ngayKetThuc, "dd/MM/yyyy", null);
                            TimeSpan time = fromDateKT.Subtract(fromDate);
                            int soNgay = time.Days;
                            string[] arrNghiePhep = new string[soNgay + 1];
                            for (int i = 0; i <= soNgay; i++)
                            {
                                arrNghiePhep[i] = fromDate.AddDays(i).ToString();
                            }
                            var dsCongTac = lqPhieuDN.tbl_NS_PhieuCongTacs.Where(d => d.maNhanVien == maNhanVien && (d.ngayLap.Value.Year == DateTime.Now.Year)).ToList();
                            foreach (var item in dsCongTac)
                            {
                                TimeSpan times = item.ngayKetThuc.Value.Subtract(item.ngayBatDau.Value);
                                int soNgays = times.Days;
                                string[] arrCongTac = new string[soNgays + 1];
                                for (int i = 0; i <= soNgays; i++)
                                {
                                    arrCongTac[i] = item.ngayBatDau.Value.AddDays(i).ToString();
                                }
                                for (int i = 0; i < arrNghiePhep.Count(); i++)
                                {
                                    for (int j = 0; j < arrCongTac.Count(); j++)
                                    {
                                        if (arrNghiePhep[i] == arrCongTac[j])
                                        {
                                            return Json(0);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Kiểm tra ngày bắt đầu
                            int count = lqPhieuDN.tbl_NS_PhieuCongTacs.Where(t => t.maNhanVien == maNhanVien && (t.ngayBatDau == fromDate || (t.ngayBatDau < fromDate && t.ngayKetThuc != null && fromDate <= t.ngayKetThuc))).Count();
                            if (count == 0)
                            {
                                return Json(1);
                            }
                            else
                            {
                                return Json(0);
                            }
                        }

                    }
                }
                return Json(1);
            }
            catch
            {
                return Json(1);
            }
        }
        #endregion

        public ActionResult CheckNgayBatDau(string ngayBatDau, string maNhanVien, int? loaiNghi, string loaiNghiPhep)
        {

            DateTime fromDate = DateTime.ParseExact(ngayBatDau, "dd/MM/yyyy", null);
            DateTime ngayLap = DateTime.Now;

            //var ls = lqPhieuDN.tbl_NS_PhieuNghiPheps.Where(t => t.maNhanVien.Equals(maNhanVien) && (t.ngayBatDau == fromDate ||
            //    (t.ngayBatDau < fromDate && t.ngayKetThuc != null && fromDate <= t.ngayKetThuc))).ToList();
            var getLoaiNghi = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == loaiNghiPhep).FirstOrDefault();
            var ls = lqPhieuDN.sp_NS_CheckNgayPhep(maNhanVien, fromDate).ToList();

            if (getLoaiNghi == null)
            {
                return Json(1);
            }
            DateTime today = DateTime.Now;
            DateTime answer = today.AddDays(Convert.ToInt32(getLoaiNghi.soNgayRangBuoc));
            // ngay answer phai lon hon hoac bang ngay bat dau

            int result = DateTime.Compare(fromDate.Date, answer.Date);
            //Kiểm tra nhân viên đó có được quyền tạo phiếu trực tiếp hay không (Admin nhân sự mới được quyền tạo phiếu trực tiếp)
            //string flag = AdminNhanSu(GetUser().manv);
            string flag = "false";
            if (lqPhieuDN.fnc_HTQuyenXemPhieu(GetUser().manv, taskIDSystem, string.Empty).Count() > 0)
            {
                flag = "true";
            }

            if (result < 0 && flag == "false")
            {
                return Json(1);
            }
            else if (ls.Count() > 0)
            {
                //return Json(2);
                if (loaiNghi == 1)
                {
                    return Json(2);
                }
                else
                {
                    if (ls.Select(d => d.loaiNgayBatDau).FirstOrDefault() != 1)
                    {
                        var Phieu = ls.Where(d => d.loaiNgayBatDau == loaiNghi).FirstOrDefault();
                        if (Phieu != null)
                        {
                            return Json(2);
                        }
                        else
                        {
                            return Json(0);
                        }
                    }
                }
            }
            return Json(0);
        }
        // Xuat File Phieu Nghi Phep
        public void XuatFilePhieuNghiPhep(int? page, string searchString, string tuNgay, string denNgay, string trangThai, string loaiNghi)
        {

            try
            {
                var userName = GetUser().manv;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                if (!String.IsNullOrEmpty(tuNgay))
                    fromDate = DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(denNgay))
                    toDate = DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                ViewBag.isGet = "True";
                var tblPhieuDeNghis = lqPhieuDN.sp_NS_PhieuDangKyNghiPhep_Index(trangThai, loaiNghi, fromDate, toDate, searchString, userName).ToList();
                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);
                filename += "XuatPhieuNghiPhep" + ".xls";


                var sheet = workbook.GetSheet("danhsachnhanvien");


                #region format style excel
                /*style title start*/
                //tạo font cho các title
                //font tiêu đề 
                HSSFFont hFontTieuDe = (HSSFFont)workbook.CreateFont();
                hFontTieuDe.FontHeightInPoints = 18;
                hFontTieuDe.Boldweight = 100 * 10;
                hFontTieuDe.FontName = "Times New Roman";
                hFontTieuDe.Color = HSSFColor.BLUE.index;

                //font tiêu đề 
                HSSFFont hFontTongGiaTriHT = (HSSFFont)workbook.CreateFont();
                hFontTongGiaTriHT.FontHeightInPoints = 11;
                hFontTongGiaTriHT.Boldweight = (short)FontBoldWeight.BOLD;
                hFontTongGiaTriHT.FontName = "Times New Roman";
                hFontTongGiaTriHT.Color = HSSFColor.BLACK.index;

                //font thông tin bảng tính
                HSSFFont hFontTT = (HSSFFont)workbook.CreateFont();
                hFontTT.IsItalic = true;
                hFontTT.Boldweight = (short)FontBoldWeight.BOLD;
                hFontTT.Color = HSSFColor.BLACK.index;
                hFontTT.FontName = "Times New Roman";
                hFontTieuDe.FontHeightInPoints = 11;

                //font chứ hoa đậm
                HSSFFont hFontNommalUpper = (HSSFFont)workbook.CreateFont();
                hFontNommalUpper.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalUpper.Color = HSSFColor.BLACK.index;
                hFontNommalUpper.FontName = "Times New Roman";

                //font chữ bình thường
                HSSFFont hFontNommal = (HSSFFont)workbook.CreateFont();
                hFontNommal.Color = HSSFColor.BLACK.index;
                hFontNommal.FontName = "Times New Roman";

                //font chữ bình thường đậm
                HSSFFont hFontNommalBold = (HSSFFont)workbook.CreateFont();
                hFontNommalBold.Color = HSSFColor.BLACK.index;
                hFontNommalBold.Boldweight = (short)FontBoldWeight.BOLD;
                hFontNommalBold.FontName = "Times New Roman";

                //tạo font cho các title end

                //Set style
                var styleTitle = workbook.CreateCellStyle();
                styleTitle.SetFont(hFontTieuDe);
                styleTitle.Alignment = HorizontalAlignment.LEFT;

                //style infomation
                var styleInfomation = workbook.CreateCellStyle();
                styleInfomation.SetFont(hFontTT);
                styleInfomation.Alignment = HorizontalAlignment.LEFT;

                //style header
                var styleheadedColumnTable = workbook.CreateCellStyle();
                styleheadedColumnTable.SetFont(hFontNommalUpper);
                styleheadedColumnTable.WrapText = true;
                styleheadedColumnTable.BorderBottom = CellBorderType.THIN;
                styleheadedColumnTable.BorderLeft = CellBorderType.THIN;
                styleheadedColumnTable.BorderRight = CellBorderType.THIN;
                styleheadedColumnTable.BorderTop = CellBorderType.THIN;
                styleheadedColumnTable.VerticalAlignment = VerticalAlignment.CENTER;
                styleheadedColumnTable.Alignment = HorizontalAlignment.CENTER;

                //style sum cell
                var styleCellSumary = workbook.CreateCellStyle();
                styleCellSumary.SetFont(hFontNommalUpper);
                styleCellSumary.WrapText = true;
                styleCellSumary.BorderBottom = CellBorderType.THIN;
                styleCellSumary.BorderLeft = CellBorderType.THIN;
                styleCellSumary.BorderRight = CellBorderType.THIN;
                styleCellSumary.BorderTop = CellBorderType.THIN;
                styleCellSumary.VerticalAlignment = VerticalAlignment.CENTER;
                styleCellSumary.Alignment = HorizontalAlignment.RIGHT;

                var styleHeading1 = workbook.CreateCellStyle();
                styleHeading1.SetFont(hFontNommalBold);
                styleHeading1.WrapText = true;
                styleHeading1.BorderBottom = CellBorderType.THIN;
                styleHeading1.BorderLeft = CellBorderType.THIN;
                styleHeading1.BorderRight = CellBorderType.THIN;
                styleHeading1.BorderTop = CellBorderType.THIN;
                styleHeading1.VerticalAlignment = VerticalAlignment.CENTER;
                styleHeading1.Alignment = HorizontalAlignment.LEFT;

                var hStyleConLeft = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConLeft.SetFont(hFontNommal);
                hStyleConLeft.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConLeft.Alignment = HorizontalAlignment.LEFT;
                hStyleConLeft.WrapText = true;
                hStyleConLeft.BorderBottom = CellBorderType.THIN;
                hStyleConLeft.BorderLeft = CellBorderType.THIN;
                hStyleConLeft.BorderRight = CellBorderType.THIN;
                hStyleConLeft.BorderTop = CellBorderType.THIN;

                var hStyleConRight = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConRight.SetFont(hFontNommal);
                hStyleConRight.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConRight.Alignment = HorizontalAlignment.RIGHT;
                hStyleConRight.BorderBottom = CellBorderType.THIN;
                hStyleConRight.BorderLeft = CellBorderType.THIN;
                hStyleConRight.BorderRight = CellBorderType.THIN;
                hStyleConRight.BorderTop = CellBorderType.THIN;


                var hStyleConCenter = (HSSFCellStyle)workbook.CreateCellStyle();
                hStyleConCenter.SetFont(hFontNommal);
                hStyleConCenter.VerticalAlignment = VerticalAlignment.TOP;
                hStyleConCenter.Alignment = HorizontalAlignment.CENTER;
                hStyleConCenter.BorderBottom = CellBorderType.THIN;
                hStyleConCenter.BorderLeft = CellBorderType.THIN;
                hStyleConCenter.BorderRight = CellBorderType.THIN;
                hStyleConCenter.BorderTop = CellBorderType.THIN;
                //set style end

                #endregion

                Row rowC = null;
                //Khai báo row đầu tiên
                int firstRowNumber = 1;

                string cellTenCty = "CÔNG TY CỔ PHẦN TV.WINDOW";
                var titleCellCty = HSSFCellUtil.CreateCell(sheet.CreateRow(0), 0, cellTenCty.ToUpper());
                titleCellCty.CellStyle = styleTitle;

                //string cellEnd1 = "Bảng Lương Tháng " + thang + " năm " + nam;
                //var titleCellEnd1 = HSSFCellUtil.CreateCell(sheet.GetRow(0), 43, cellEnd1.ToUpper());
                //titleCellEnd1.CellStyle = styleTitle;

                string rowtitle = "Danh sách phiếu nghỉ phép";
                var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 6, rowtitle.ToUpper());
                titleCell.CellStyle = styleTitle;

                //string cellEnd2 = "";
                //var titleCellEnd2 = HSSFCellUtil.CreateCell(sheet.GetRow(1), 43, cellEnd2.ToUpper());
                //titleCellEnd2.CellStyle = styleTitle;

                ++firstRowNumber;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Số phiếu");
                list1.Add("Mã nhân viên");
                list1.Add("Họ tên");
                list1.Add("Phòng ban");
                list1.Add("Chức danh");

                list1.Add("Loại nghỉ");
                list1.Add("Số ngày nghỉ");
                list1.Add("Lý do");
                list1.Add("Ngày nghỉ");

                list1.Add("Trạng thái phiếu");




                var idRowStart = 2; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);


                idRowStart++;
                var headerRow2 = sheet.CreateRow(idRowStart);




                sheet.SetColumnWidth(0, 8 * 210);
                sheet.SetColumnWidth(1, 20 * 210);
                sheet.SetColumnWidth(2, 20 * 310);
                sheet.SetColumnWidth(3, 20 * 210);
                sheet.SetColumnWidth(4, 20 * 210);
                sheet.SetColumnWidth(5, 20 * 210);
                sheet.SetColumnWidth(6, 30 * 350);
                sheet.SetColumnWidth(7, 20 * 210);
                sheet.SetColumnWidth(8, 20 * 210);
                sheet.SetColumnWidth(9, 20 * 210);
                sheet.SetColumnWidth(10, 20 * 210);
                sheet.SetColumnWidth(11, 20 * 210);
                sheet.SetColumnWidth(12, 20 * 210);
                sheet.SetColumnWidth(13, 20 * 210);



                var stt = 0;
                int dem = 0;
                if (tblPhieuDeNghis.Count > 0)
                {
                    foreach (var item in tblPhieuDeNghis)
                    {

                        dem = 0;

                        idRowStart++;
                        rowC = sheet.CreateRow(idRowStart);
                        string ngayNghi = item.ngayKetThuc != null ? Convert.ToString(String.Format("{0: dd/MM/yyyy}", item.ngayBatDau)) + "-" + Convert.ToString(String.Format("{0: dd/MM/yyyy}", item.ngayKetThuc)) : Convert.ToString(String.Format("{0: dd/MM/yyyy}", item.ngayBatDau));
                        ReportHelperExcel.SetAlignment(rowC, dem++, (++stt).ToString(), hStyleConCenter);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.maPhieu, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.maNhanVien, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.hoTen, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.tenPhongBan, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.TenChucDanh, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.tenLoaiNghiPhep, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(item.soNgayNghiPhepThucTe), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(Regex.Replace(item.lyDo, "<.*?>", String.Empty)), hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(ngayNghi), hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, item.tenTrangThaiDuyet, hStyleConLeft);
                    }
                }


                idRowStart = idRowStart + 2;
                string cellFooterGD = "GIÁM ĐỐC NHÂN SỰ-QTVP";
                var titleCellFooterGD = HSSFCellUtil.CreateCell(sheet.CreateRow(idRowStart), 1, cellFooterGD.ToUpper());
                titleCellFooterGD.CellStyle = styleTitle;

                string cellFooterKT = "PHÒNG TÀI CHÍNH - KẾ TOÁN";
                var titleCellFooterKT = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 5, cellFooterKT.ToUpper());
                titleCellFooterKT.CellStyle = styleTitle;

                string cellFooterTGD = "TỔNG GIÁM ĐỐC";
                var titleCellFooterTGD = HSSFCellUtil.CreateCell(sheet.GetRow(idRowStart), 9, cellFooterTGD.ToUpper());
                titleCellFooterTGD.CellStyle = styleTitle;

                var stream = new MemoryStream();
                workbook.Write(stream);

                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
                Response.Clear();

                Response.BinaryWrite(stream.GetBuffer());
                Response.End();
            }
            catch
            {

            }

        }

        public JsonResult CountDayPhepNam(string code, string ngayNghi)
        {
            try
            {
                double phepNamConLai = 0.0;
                double phepTonConLai = 0.0;

                var objNghiPhep = linqNS.sp_PhepNamNhanVien_Index(DateTime.Now.Year, null, code).FirstOrDefault();
                //lqPhieuDN.sp_DN_rptThongKePhepNam_PhieuNghiPhep(code).FirstOrDefault();
                if (objNghiPhep != null)
                {
                    phepNamConLai = (double)objNghiPhep.soNgayPhepConLaiTrongNam;
                    phepTonConLai = (double)objNghiPhep.phepTonDuocDuyet;
                }
                return Json(new { SoPhepNamCon = phepNamConLai, SoPhepTonCon = phepTonConLai });
            }
            catch (Exception ex)
            {
                //write log
                Log4Net.WriteLog(log4net.Core.Level.Error, ex.Message);

                return Json(null);
            }

        }


        public double CountDayPhepNamControl(string code)
        {

            double phepNamConLai = (double?)linqNS.sp_PhepNamNhanVien_Index(DateTime.Now.Year, null, code).Where(d => d.maNhanVien == code).Select(d => d.soNgayPhepConLaiTrongNam).FirstOrDefault() ?? 0;
            return phepNamConLai;
        }
        // Tinh lai ngay nghi phep
        public double CountNgayNghiPhep(string code, string fromDateVDA, string toDateVDA, double? halftStartDay, double? halfEndDay)
        {
            try
            {

                toDateVDA = string.IsNullOrEmpty(toDateVDA) ? fromDateVDA : toDateVDA;
                //string[] splitFromDate = fromDateVDA.Split('/');
                //string[] splitToDate = toDateVDA.Split('/');
                DateTime? fromDate = null;
                if (!string.IsNullOrEmpty(fromDateVDA))
                {
                    fromDate = DateTime.ParseExact(fromDateVDA, "dd/MM/yyyy", null);
                }
                DateTime? toDate = null;
                if (!string.IsNullOrEmpty(toDateVDA))
                {
                    toDate = DateTime.ParseExact(toDateVDA, "dd/MM/yyyy", null);
                }
                double? count = linqDM.Fn_SoNgayThucNghiMoi(fromDate, halftStartDay, toDate, halfEndDay, code);
                if (count >= 0)
                {

                    return count ?? 0;
                }
                else
                {
                    TimeSpan timeSpan = toDate.Value.Date - fromDate.Value.Date;
                    if (timeSpan.Days > 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public JsonResult CountDayHoLiDay(string code, string fromDateVDA, string toDateVDA, double? halftStartDay, double? halfEndDay)
        {
            try
            {

                toDateVDA = string.IsNullOrEmpty(toDateVDA) ? fromDateVDA : toDateVDA;
                //string[] splitFromDate = fromDateVDA.Split('/');
                //string[] splitToDate = toDateVDA.Split('/');
                DateTime? fromDate = null;
                if (!string.IsNullOrEmpty(fromDateVDA))
                {
                    fromDate = DateTime.ParseExact(fromDateVDA, "dd/MM/yyyy", null);
                }
                DateTime? toDate = null;
                if (!string.IsNullOrEmpty(toDateVDA))
                {
                    toDate = DateTime.ParseExact(toDateVDA, "dd/MM/yyyy", null);
                }
                double? count = linqDM.Fn_SoNgayThucNghiMoi(fromDate, halftStartDay, toDate, halfEndDay, code);
                if (count >= 0)
                {

                    return Json(new { CountDay = count });
                }
                else
                {
                    TimeSpan timeSpan = toDate.Value.Date - fromDate.Value.Date;
                    if (timeSpan.Days > 0)
                    {
                        return Json(new { strHtml = "Bạn chưa được phân ca vui lòng liên hệ với nhân sự." });
                    }
                    else
                    {
                        return Json(new { message = false, strHtml = "Ngày kết thúc nghỉ phải lớn hơn ngày bắt đầu nghỉ." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }
        private string GetChiNhanh(string maNV)
        {
            var chiNhanh = (from p in linqNS.tbl_NS_NhanVienChiNhanhs
                            join t in linqNS.GetTable<Sys_ChiNhanhVanPhong>() on p.maChiNhanh equals t.maChiNhanh
                            where p.maNhanVien == maNV
                            orderby p.ngayLap descending
                            select t).FirstOrDefault();
            return (chiNhanh == null ? string.Empty : chiNhanh.tenChiNhanh) ?? string.Empty;
        }
        public string GetMax()
        {
            return lqPhieuDN.tbl_NS_PhieuNghiPheps.OrderByDescending(d => d.ID).Select(d => d.maPhieu).FirstOrDefault();
        }

        private void BindDataLeave(string maLoaiNghi, string maNhaVien)
        {
            List<tbl_DM_LoaiNghiPhep> leaves = new List<tbl_DM_LoaiNghiPhep>();
            List<tbl_DM_LoaiNghiPhep> leaves1 = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.trangThai == true).ToList();
            //linqDM.tbl_DM_LoaiNghiPheps.Where(e => e.maLoaiNghiPhep == "WEDLEAVE" || e.maLoaiNghiPhep == "YEARLEAVE" || e.maLoaiNghiPhep == "UNPAIDLEAVE" || e.maLoaiNghiPhep == "OTHERLEAVE" || e.maLoaiNghiPhep == "FUNERALLEAVE").ToList();
            tbl_DM_LoaiNghiPhep lea = new tbl_DM_LoaiNghiPhep();
            lea.id = -1;
            lea.maLoaiNghiPhep = "";
            lea.tenLoaiNghiPhep = "Chọn";
            leaves.Add(lea);
            leaves.AddRange(leaves1);

            ViewBag.LoaiNghi = new SelectList(leaves, "maLoaiNghiPhep", "tenLoaiNghiPhep", maLoaiNghi);
        }
        private void GetHaftDayStart(string selectedValue)
        {
            Dictionary<string, string> dics = new Dictionary<string, string>();
            dics["1"] = "Cả ngày";
            dics["2"] = "Nửa ngày(buổi sáng)";
            dics["3"] = "Nửa ngày(buổi chiều)";

            ViewBag.LoaiNghiBatDau = new SelectList(dics, "Key", "Value", selectedValue);
        }
        private void GetHaftDayEnd(string selectedValue)
        {
            Dictionary<string, string> dics = new Dictionary<string, string>();
            dics["1"] = "Cả ngày";
            dics["2"] = "Nửa ngày(buổi sáng)";
            dics["3"] = "Nửa ngày(buổi chiều)";
            ViewBag.LoaiNghiKetThuc = new SelectList(dics, "Key", "Value", selectedValue);
        }

        #region Duyệt qui trình động
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ViewsApproval(string id)
        {
            try
            {
                return RedirectToAction("Details", new { id = id });// detail de duyet
            }
            catch
            {
                return View();
            }
        }


        /// <summary>
        /// Duyệt theo qui trình động phiếu nghỉ phép
        /// </summary>
        /// <param name="maPhieu"></param>
        /// <param name="maQuiTrinh"></param>
        /// <param name="sendMail"></param>
        /// <param name="sendSMS"></param>
        /// <returns></returns>
        public ActionResult DuyetTheoQuiTrinhDong(string maPhieu, string maCongViec, bool sendMail, bool sendSMS, string maNhanVien, int soNgayNghiPhep, string lyDo, int idQuiTrinh)
        {
            try
            {
                LinqHeThongDataContext ht = new LinqHeThongDataContext();
                string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
                DMNguoiDuyetController _nguoiDuyet = new DMNguoiDuyetController();
                var kq = _nguoiDuyet.DuyeTheoQuiTrinhDong(maPhieu, maCongViec, sendMail, sendSMS, GetUser().manv, hoTen, soNgayNghiPhep, lyDo, idQuiTrinh, string.Empty);
                return kq;
            }
            catch (Exception e)
            {
                return Json("Lỗi: " + e.Message);
            }
        }
        #endregion

        /// <summary>
        /// Danh sách phòng ban
        /// </summary>
        /// <returns></returns>
        public ActionResult ChonNhanVien()
        {
            StringBuilder buildTree = new StringBuilder();
            var phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_ChonNhanVien");
        }

        /// <summary>
        /// Danh sách nhân viên phòng ban
        /// </summary>
        /// <param name="page"></param>
        /// <param name="searchString"></param>
        /// <param name="maPhongBan"></param>
        /// <returns></returns>
        public ActionResult LoadNhanVien(int? page, string searchString, string maPhongBan)
        {
            IList<sp_PB_DanhSachNhanVienResult> phongBan1s;
            phongBan1s = linqDM.sp_PB_DanhSachNhanVien(searchString, maPhongBan).ToList();
            ViewBag.isGet = "True";
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = phongBan1s.Count();
            ViewBag.Search = searchString;
            ViewBag.MaPhongBan = maPhongBan;
            return PartialView("_LoadNhanVien", phongBan1s.ToPagedList(currentPageIndex, 10));
        }
    }
}
