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

namespace BatDongSan.Controllers.PhieuDeNghi
{
    public class PhieuCongTacController : ApplicationController
    {
        LinqPhieuDeNghiDataContext lqPhieuDN = new LinqPhieuDeNghiDataContext();
        LinqNhanSuDataContext linqNS = new LinqNhanSuDataContext();
        LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
        tbl_NS_PhieuCongTac tblPhieuDeNghi;
        IList<tbl_NS_PhieuCongTac> tblPhieuDeNghis;
        PhieuDeNghiCongTac PhieuDeNghiModel;
        public const string taskIDSystem = "PhieuCongTac";//REGWORKVOTE
        public bool? permission;
        //
        // GET: /PhieuCongTac/

        public ActionResult Index(int? page, string searchString, string tuNgay, string denNgay, string trangThai)
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
                BindDataTrangThai(taskIDSystem);
                Administrator(GetUser().manv);
                var userName = GetUser().manv;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                if (!String.IsNullOrEmpty(tuNgay))
                    fromDate = DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(denNgay))
                    toDate = DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                ViewBag.isGet = "True";
                var tblPhieuDeNghis = lqPhieuDN.sp_NS_PhieuDangKyCongTac_Index(fromDate, toDate, userName, trangThai, searchString,null).ToList();
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Count = tblPhieuDeNghis.Count();
                ViewBag.Search = searchString;
                ViewBag.tuNgay = tuNgay;
                ViewBag.denNgay = tuNgay;
                ViewBag.trangThai = trangThai;
                return View(tblPhieuDeNghis.ToPagedList(currentPageIndex, 50));
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }
        public ActionResult ViewIndex(int? page, string qSearch, string tuNgay, string denNgay, int? flagViPham, string trangThai)
        {
            try
            {
                ViewBag.QuyenDuyet = GetPermission(taskIDSystem, BangPhanQuyen.QuyenDuyet);
                Administrator(GetUser().manv);
                var userName = GetUser().manv;
                DateTime? fromDate = null;
                DateTime? toDate = null;
                if (!String.IsNullOrEmpty(tuNgay))
                    fromDate = String.IsNullOrEmpty(tuNgay) ? (DateTime?)null : DateTime.ParseExact(tuNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                if (!String.IsNullOrEmpty(denNgay))
                    toDate = String.IsNullOrEmpty(denNgay) ? (DateTime?)null : DateTime.ParseExact(denNgay, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                ViewBag.isGet = "True";
                var tblPhieuDeNghis = lqPhieuDN.sp_NS_PhieuDangKyCongTac_Index(fromDate, toDate, userName, trangThai, qSearch,flagViPham).ToList();
                int currentPageIndex = page.HasValue ? page.Value : 1;
                ViewBag.Count = tblPhieuDeNghis.Count();
                ViewBag.Search = qSearch;
                ViewBag.tuNgay = tuNgay;
                ViewBag.denNgay = denNgay;
                ViewBag.trangThai = trangThai;
                return PartialView("ViewIndex", tblPhieuDeNghis.ToPagedList(currentPageIndex, 50));
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }

        }

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
                var tblLoaiNghiPhep = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == "PhieuCongTac").FirstOrDefault();
                PhieuDeNghiModel = new PhieuDeNghiCongTac();
                PhieuDeNghiModel.maPhieu = GenerateUtil.CheckLetter("DKCT", GetMax());
                PhieuDeNghiModel.ngayLap = DateTime.Now;
                PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
                {
                    maNhanVien = GetUser().manv,
                    hoVaTen = HoVaTen(GetUser().manv)
                };
                PhieuDeNghiModel.ngayBatDau = DateTime.Now;
                PhieuDeNghiModel.ngayKetThuc = DateTime.Now;
                PhieuDeNghiModel.soNgayCongTac = 1;
                PhieuDeNghiModel.phuCap = 0;
                PhieuDeNghiModel.soNgayRangBuoc = tblLoaiNghiPhep.soNgayRangBuoc ?? 0;
                buitlTinhThanh(string.Empty);
                buitlQuocGia(string.Empty);
                //Kiểm tra nhân viên đó có được quyền tạo phiếu trực tiếp hay không (Admin nhân sự mới được quyền tạo phiếu trực tiếp)
                // string flag = AdminNhanSu(GetUser().manv);
                string flag = "false";
                if (lqPhieuDN.fnc_HTQuyenXemPhieu(GetUser().manv, taskIDSystem, string.Empty).Count() > 0)
                {
                    flag = "true";
                }
                if (flag == "true")
                {
                    PhieuDeNghiModel.NhanVienLapPhieuTT = new NhanVienModel
                    {
                        maNhanVien = GetUser().manv,
                        hoVaTen = HoVaTen(GetUser().manv)
                    };

                    PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
                    {
                        maNhanVien = GetUser().manv,
                        hoVaTen = HoVaTen(GetUser().manv),
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


                tblPhieuDeNghi = new tbl_NS_PhieuCongTac();

                //Kiểm tra ngày kết thúc so với làm phiếu trong ngày
                tblPhieuDeNghi.ngayKetThuc = String.IsNullOrEmpty(coll.Get("ngayKetThuc")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayKetThuc"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tblPhieuDeNghi.ngayLap = DateTime.Now;
                tblPhieuDeNghi.maNhanVien = coll.Get("NhanVien.maNhanVien");
                tblPhieuDeNghi.nguoiLap = GetUser().manv;
                tblPhieuDeNghi.ngayBatDau = String.IsNullOrEmpty(coll.Get("ngayBatDau")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayBatDau"), "dd/MM/yyyy", CultureInfo.InvariantCulture);

                tblPhieuDeNghi.soNgayCongTac = Convert.ToInt32(coll.Get("soNgayCongTac"));
                tblPhieuDeNghi.phuCap = Convert.ToDecimal(coll.Get("phuCap"));
                try
                {
                    string date = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
                    tblPhieuDeNghi.gioBatDau = Convert.ToDateTime(date + " " + coll.Get("gioBatDau"));
                    tblPhieuDeNghi.gioKetThuc = Convert.ToDateTime(date + " " + coll.Get("gioKetThuc"));
                }
                catch
                {
                    string date = String.Format("{0:MM/dd/yyyy}", DateTime.Now);
                    tblPhieuDeNghi.gioBatDau = Convert.ToDateTime(date + " " + coll.Get("gioBatDau"));
                    tblPhieuDeNghi.gioKetThuc = Convert.ToDateTime(date + " " + coll.Get("gioKetThuc"));
                }

                //tblPhieuDeNghi.loaiCongTac = Convert.ToBoolean(coll.Get("loaiCongTac"));

                //if (!string.IsNullOrEmpty(coll.Get("maTinh")))
                //{
                //    tblPhieuDeNghi.noiCongTac = coll.Get("maTinh");{
                //}
                //if (!string.IsNullOrEmpty(coll.Get("maQuocGia")))
                //{
                //    tblPhieuDeNghi.quocGiaCongTac = coll.Get("maQuocGia");
                //}
                tblPhieuDeNghi.lyDo = coll.Get("lyDo");
                tblPhieuDeNghi.phuongTien = coll.Get("phuongTien");
                tblPhieuDeNghi.trangThai = 0;
                tblPhieuDeNghi.ghiChu = coll.Get("ghiChu");

                tblPhieuDeNghi.phongBanCongTac = coll.Get("phongBanCongTac");
                tblPhieuDeNghi.maPhieu = GenerateUtil.CheckLetter("DKCT", GetMax());
                lqPhieuDN.tbl_NS_PhieuCongTacs.InsertOnSubmit(tblPhieuDeNghi);

                if (ChecNgayLamPhieuTrongNgay(coll.Get("ngayKetThuc")) == 1)
                {
                    //ViewData["Message"] = "Chỉ được làm phiếu trong vòng số ngày cho phép, sau khi đăng ký công tác";
                    //return View("error");
                    LuuLaiSoNgayVuotQuaQuiDinh(tblPhieuDeNghi);
                }
                lqPhieuDN.SubmitChanges();

                return RedirectToAction("Edit", new { id = tblPhieuDeNghi.maPhieu });
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
                var phieu = lqPhieuDN.tbl_NS_PhieuCongTacs.Where(d => d.maPhieu == id).FirstOrDefault();
                lqPhieuDN.tbl_NS_PhieuCongTacs.DeleteOnSubmit(phieu);
                LuuLichSuCapNhatPhieu(id, taskIDSystem, 1);
                LinqHeThongDataContext lqHT = new LinqHeThongDataContext();
                var nguoiDuyet = lqHT.tbl_HT_DMNguoiDuyets.Where(d => d.maPhieu == id);
                if (nguoiDuyet != null && nguoiDuyet.Count() > 0)
                {
                    lqHT.tbl_HT_DMNguoiDuyets.DeleteAllOnSubmit(nguoiDuyet);
                }

                var delPhieuViPham = lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.Where(d => d.maPhieu == id);
                lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.DeleteAllOnSubmit(delPhieuViPham);

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
                var tblLoaiNghiPhep = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == "PhieuCongTac").FirstOrDefault();
                var dataPhieuCongTac = lqPhieuDN.tbl_NS_PhieuCongTacs.Where(d => d.maPhieu == id).FirstOrDefault();
                PhieuDeNghiModel = new PhieuDeNghiCongTac();
                PhieuDeNghiModel.maPhieu = dataPhieuCongTac.maPhieu;
                PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
                PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
                PhieuDeNghiModel.soNgayRangBuoc = tblLoaiNghiPhep.soNgayRangBuoc ?? 0;
                PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
                {
                    maNhanVien = dataPhieuCongTac.maNhanVien,
                    hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien)
                };

                PhieuDeNghiModel.NhanVienLapPhieuTT = new NhanVienModel
                {
                    maNhanVien = dataPhieuCongTac.nguoiLap,
                    hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap),
                };
                PhieuDeNghiModel.ngayBatDau = dataPhieuCongTac.ngayBatDau;
                PhieuDeNghiModel.ngayKetThuc = dataPhieuCongTac.ngayKetThuc;
                PhieuDeNghiModel.soNgayCongTac = dataPhieuCongTac.soNgayCongTac;
                PhieuDeNghiModel.phuCap = dataPhieuCongTac.phuCap;
                PhieuDeNghiModel.loaiCongTac = dataPhieuCongTac.loaiCongTac;
                PhieuDeNghiModel.gioBatDau = dataPhieuCongTac.gioBatDau;
                PhieuDeNghiModel.gioKetThuc = dataPhieuCongTac.gioKetThuc;

                PhieuDeNghiModel.maTinh = dataPhieuCongTac.noiCongTac;
                PhieuDeNghiModel.maQuocGia = dataPhieuCongTac.quocGiaCongTac;

                PhieuDeNghiModel.phongBanCongTac = dataPhieuCongTac.phongBanCongTac;
                PhieuDeNghiModel.tenPhongBan = linqDM.tbl_DM_PhongBans.Where(d => d.maPhongBan == dataPhieuCongTac.phongBanCongTac).Select(d => d.tenPhongBan).FirstOrDefault();
                PhieuDeNghiModel.phuongTien = dataPhieuCongTac.phuongTien;
                PhieuDeNghiModel.lyDo = dataPhieuCongTac.lyDo;
                PhieuDeNghiModel.ghiChu = dataPhieuCongTac.ghiChu;
                PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
                buitlTinhThanh(dataPhieuCongTac.noiCongTac);
                buitlQuocGia(dataPhieuCongTac.quocGiaCongTac);

                DMNguoiDuyetController nd = new DMNguoiDuyetController();
                PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(PhieuDeNghiModel.maPhieu, PhieuDeNghiModel.maQuiTrinhDuyet);
                LinqHeThongDataContext ht = new LinqHeThongDataContext();
                string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
                ViewBag.HoTen = hoTen;
                int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
                ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
                ViewBag.URL = Request.Url.AbsoluteUri.ToString();
                Administrator(GetUser().manv);

                var phieuViPham = lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.Where(d => d.maPhieu == PhieuDeNghiModel.maPhieu).FirstOrDefault();
                if (phieuViPham != null)
                {
                    ViewBag.ViPham = "1";
                }
                ViewBag.SoLanViPham = (lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.Where(d => d.maCongViec == "PhieuCongTac" && d.maNhanVien == dataPhieuCongTac.maNhanVien).Count()).ToString();
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

                tblPhieuDeNghi = lqPhieuDN.tbl_NS_PhieuCongTacs.Where(d => d.maPhieu == id).FirstOrDefault();

                tblPhieuDeNghi.ngayBatDau = String.IsNullOrEmpty(coll.Get("ngayBatDau")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayBatDau"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tblPhieuDeNghi.ngayKetThuc = String.IsNullOrEmpty(coll.Get("ngayKetThuc")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayKetThuc"), "dd/MM/yyyy", CultureInfo.InvariantCulture);

                tblPhieuDeNghi.soNgayCongTac = Convert.ToInt32(coll.Get("soNgayCongTac"));
                tblPhieuDeNghi.phuCap = Convert.ToDecimal(coll.Get("phuCap"));

                try
                {
                    string date = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
                    tblPhieuDeNghi.gioBatDau = Convert.ToDateTime(date + " " + coll.Get("gioBatDau"));
                    tblPhieuDeNghi.gioKetThuc = Convert.ToDateTime(date + " " + coll.Get("gioKetThuc"));
                }
                catch
                {
                    string date = String.Format("{0:MM/dd/yyyy}", DateTime.Now);
                    tblPhieuDeNghi.gioBatDau = Convert.ToDateTime(date + " " + coll.Get("gioBatDau"));
                    tblPhieuDeNghi.gioKetThuc = Convert.ToDateTime(date + " " + coll.Get("gioKetThuc"));
                }
                //tblPhieuDeNghi.loaiCongTac = Convert.ToBoolean(coll.Get("loaiCongTac"));

                //if (!string.IsNullOrEmpty(coll.Get("maTinh")))
                //{
                //    tblPhieuDeNghi.quocGiaCongTac = null;
                //    tblPhieuDeNghi.noiCongTac = coll.Get("maTinh");
                //}
                //if (!string.IsNullOrEmpty(coll.Get("maQuocGia")))
                //{
                //    tblPhieuDeNghi.noiCongTac = null;
                //    tblPhieuDeNghi.quocGiaCongTac = coll.Get("maQuocGia");
                //}
                tblPhieuDeNghi.lyDo = coll.Get("lyDo");
                tblPhieuDeNghi.phuongTien = coll.Get("phuongTien");
                tblPhieuDeNghi.trangThai = 0;
                tblPhieuDeNghi.ghiChu = coll.Get("ghiChu");

                tblPhieuDeNghi.phongBanCongTac = coll.Get("phongBanCongTac");
                LuuLichSuCapNhatPhieu(id, taskIDSystem, 0);

                if (ChecNgayLamPhieuTrongNgay(coll.Get("ngayKetThuc")) == 1)
                {
                    //ViewData["Message"] = "Chỉ được làm phiếu trong vòng số ngày cho phép, sau khi đăng ký công tác";
                    //return View("error");
                    LuuLaiSoNgayVuotQuaQuiDinh(tblPhieuDeNghi);
                }
                else
                {
                    var delPVP = lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.Where(d => d.maPhieu == tblPhieuDeNghi.maPhieu);
                    lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.DeleteAllOnSubmit(delPVP);
                }
                lqPhieuDN.SubmitChanges();

                return RedirectToAction("Edit", new { id = tblPhieuDeNghi.maPhieu });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }


        public ActionResult checkNgayBatDau(string ngayBatDau, string maNhanVien)
        {
            return Json(ChecNgayLamPhieuTrongNgay(ngayBatDau));
        }

        public int ChecNgayLamPhieuTrongNgay(string ngayBatDau)
        {
            var getLoaiNghi = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == "PhieuCongTac").FirstOrDefault();
            if (getLoaiNghi == null) return 1;
            //ngày đi công tác
            DateTime fromDate = DateTime.ParseExact(ngayBatDau, "dd/MM/yyyy", null);
            //ngày hiện tại
            DateTime today = DateTime.Now;
            //ngày hiện tại cộng cho số ngày không được vượt quá ngày đi công tác
            //DateTime answer = fromDate.AddDays(getLoaiNghi.soNgayRangBuoc ?? 0);
            DateTime answer = fromDate.AddDays(getLoaiNghi.soNgayRangBuoc ?? 0);
            // ngay answer phai lon hon hoac bang ngay bat dau

            int result = DateTime.Compare(answer.Date, today.Date);
            string flag = "false";
            if (lqPhieuDN.fnc_HTQuyenXemPhieu(GetUser().manv, taskIDSystem, string.Empty).Count() > 0)
            {
                flag = "true";
            }
            //if (result == -1 && flag == "false")
            if (result == -1)
            {
                return 1;//lỗi
            }
            return 0;
        }

        public JsonResult Generate(string dateOne, string dateTwo, string maNhanVien)
        {
            try
            {
                DateTime? startDate = DateTime.ParseExact(dateOne, "dd/MM/yyyy", null);
                DateTime? endDate = DateTime.ParseExact(dateTwo, "dd/MM/yyyy", null);

                var soNgay = lqPhieuDN.fn_NS_TinhSoNgayNghi_TuNgayDenNgay_TheoCongChuan(startDate, endDate, maNhanVien) ?? 0;

                return Json(new { soNgay = soNgay + 1 });
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult Details(string id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            var tblLoaiNghiPhep = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == "PhieuCongTac").FirstOrDefault();
            var dataPhieuCongTac = lqPhieuDN.tbl_NS_PhieuCongTacs.Where(d => d.maPhieu == id).FirstOrDefault();
            PhieuDeNghiModel = new PhieuDeNghiCongTac();
            PhieuDeNghiModel.maPhieu = dataPhieuCongTac.maPhieu;
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.nguoiLap = dataPhieuCongTac.nguoiLap;
            PhieuDeNghiModel.soNgayRangBuoc = tblLoaiNghiPhep.soNgayRangBuoc ?? 0;
            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.maNhanVien,
                hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien)
            };
            PhieuDeNghiModel.NhanVienLapPhieuTT = new NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.nguoiLap,
                hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap),
            };
            PhieuDeNghiModel.ngayBatDau = dataPhieuCongTac.ngayBatDau;
            PhieuDeNghiModel.ngayKetThuc = dataPhieuCongTac.ngayKetThuc;
            PhieuDeNghiModel.soNgayCongTac = dataPhieuCongTac.soNgayCongTac;
            PhieuDeNghiModel.phuCap = dataPhieuCongTac.phuCap;
            PhieuDeNghiModel.loaiCongTac = dataPhieuCongTac.loaiCongTac;
            PhieuDeNghiModel.gioBatDau = dataPhieuCongTac.gioBatDau;
            PhieuDeNghiModel.gioKetThuc = dataPhieuCongTac.gioKetThuc;

            PhieuDeNghiModel.maTinh = dataPhieuCongTac.noiCongTac;
            PhieuDeNghiModel.tenTinh = linqNS.Sys_TinhThanhs.Where(d => d.maTinhThanh == dataPhieuCongTac.noiCongTac).Select(d => d.tenTinhThanh).FirstOrDefault();
            PhieuDeNghiModel.maQuocGia = dataPhieuCongTac.quocGiaCongTac;
            PhieuDeNghiModel.tenQuocGia = linqNS.Sys_QuocTiches.Where(d => d.maQuocTich == dataPhieuCongTac.quocGiaCongTac).Select(d => d.tenQuocTich).FirstOrDefault();

            PhieuDeNghiModel.phongBanCongTac = dataPhieuCongTac.phongBanCongTac;
            PhieuDeNghiModel.tenPhongBan = linqDM.tbl_DM_PhongBans.Where(d => d.maPhongBan == dataPhieuCongTac.phongBanCongTac).Select(d => d.tenPhongBan).FirstOrDefault();
            PhieuDeNghiModel.phuongTien = dataPhieuCongTac.phuongTien;
            PhieuDeNghiModel.lyDo = dataPhieuCongTac.lyDo;
            PhieuDeNghiModel.ghiChu = dataPhieuCongTac.ghiChu;
            PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
            buitlTinhThanh(dataPhieuCongTac.noiCongTac);
            buitlQuocGia(dataPhieuCongTac.quocGiaCongTac);

            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(PhieuDeNghiModel.maPhieu, PhieuDeNghiModel.maQuiTrinhDuyet);
            LinqHeThongDataContext ht = new LinqHeThongDataContext();
            string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
            ViewBag.HoTen = hoTen;
            int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id).Select(d => d.trangThai).FirstOrDefault() ?? 0;
            ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            //Tất cả Nhân viên phòng nhân sự duyệt
            NhanVienQLNSDuyet();
            var phieuViPham = lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.Where(d => d.maPhieu == PhieuDeNghiModel.maPhieu).FirstOrDefault();
            if (phieuViPham != null)
            {
                ViewBag.ViPham = "1";
            }
            ViewBag.SoLanViPham = (lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.Where(d => d.maCongViec == "PhieuCongTac" && d.maNhanVien == dataPhieuCongTac.maNhanVien).Count()).ToString();
            return View(PhieuDeNghiModel);
        }

        public ActionResult CreateTrucTiep()
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenThemTrucTiep);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            PhieuDeNghiModel = new PhieuDeNghiCongTac();
            PhieuDeNghiModel.maPhieu = GenerateUtil.CheckLetter("DKCT", GetMax());
            PhieuDeNghiModel.ngayLap = DateTime.Now;
            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                //maNhanVien = GetUser().manv,
                //hoVaTen = HoVaTen(GetUser().manv)
            };
            PhieuDeNghiModel.ngayBatDau = DateTime.Now;
            PhieuDeNghiModel.ngayKetThuc = DateTime.Now;
            PhieuDeNghiModel.soNgayCongTac = 1;
            PhieuDeNghiModel.phuCap = 0;

            buitlTinhThanh(string.Empty);
            buitlQuocGia(string.Empty);

            return View(PhieuDeNghiModel);
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateTrucTiep(FormCollection coll)
        {
            try
            {
                tblPhieuDeNghi = new tbl_NS_PhieuCongTac();
                tblPhieuDeNghi.maPhieu = GenerateUtil.CheckLetter("DKCT", GetMax());
                tblPhieuDeNghi.ngayLap = DateTime.Now;
                tblPhieuDeNghi.maNhanVien = coll.Get("NhanVien.maNhanVien");
                tblPhieuDeNghi.nguoiLap = GetUser().manv;
                tblPhieuDeNghi.ngayBatDau = String.IsNullOrEmpty(coll.Get("ngayBatDau")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayBatDau"), "dd/MM/yyyy", CultureInfo.InvariantCulture);

                tblPhieuDeNghi.ngayKetThuc = String.IsNullOrEmpty(coll.Get("ngayKetThuc")) ? (DateTime?)null : DateTime.ParseExact(coll.Get("ngayKetThuc"), "dd/MM/yyyy", CultureInfo.InvariantCulture);


                tblPhieuDeNghi.soNgayCongTac = Convert.ToInt32(coll.Get("soNgayCongTac"));
                tblPhieuDeNghi.phuCap = Convert.ToDecimal(coll.Get("phuCap"));
                try
                {
                    string date = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
                    tblPhieuDeNghi.gioBatDau = Convert.ToDateTime(date + " " + coll.Get("gioBatDau"));
                    tblPhieuDeNghi.gioKetThuc = Convert.ToDateTime(date + " " + coll.Get("gioKetThuc"));
                }
                catch
                {
                    string date = String.Format("{0:MM/dd/yyyy}", DateTime.Now);
                    tblPhieuDeNghi.gioBatDau = Convert.ToDateTime(date + " " + coll.Get("gioBatDau"));
                    tblPhieuDeNghi.gioKetThuc = Convert.ToDateTime(date + " " + coll.Get("gioKetThuc"));
                }

                //tblPhieuDeNghi.loaiCongTac = Convert.ToBoolean(coll.Get("loaiCongTac"));

                //if (!string.IsNullOrEmpty(coll.Get("maTinh")))
                //{
                //    tblPhieuDeNghi.noiCongTac = coll.Get("maTinh");
                //}
                //if (!string.IsNullOrEmpty(coll.Get("maQuocGia")))
                //{
                //    tblPhieuDeNghi.quocGiaCongTac = coll.Get("maQuocGia");
                //}
                tblPhieuDeNghi.lyDo = coll.Get("lyDo");
                tblPhieuDeNghi.phuongTien = coll.Get("phuongTien");
                tblPhieuDeNghi.trangThai = 1;
                tblPhieuDeNghi.ghiChu = coll.Get("ghiChu");

                tblPhieuDeNghi.phongBanCongTac = coll.Get("phongBanCongTac");

                lqPhieuDN.tbl_NS_PhieuCongTacs.InsertOnSubmit(tblPhieuDeNghi);
                lqPhieuDN.SubmitChanges();



                //DMNguoiDuyet record = new DMNguoiDuyet();
                //record.maPhieu = tblPhieuDeNghi.maPhieu;
                //record.ngayDuyet = DateTime.Now.Date;
                //record.maCongViec = "RECEIVEREGWORK";
                //record.trangThai = 4;
                //record.nguoiDuyet = new Models.NhanSu.NhanVienModel{maNhanVien=GetUser().manv,hoVaTen = hovatn}
                //new SqlDMNguoiDuyetRepository().Save(record);

                return RedirectToAction("Edit", new { id = tblPhieuDeNghi.maPhieu });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        public ActionResult ChonNhanVien()
        {
            StringBuilder buildTree = new StringBuilder();
            var phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_ChonNhanVien");
        }

        /// <summary>
        /// Danh sách nhân viên theo phòng ban
        /// </summary>
        /// <returns></returns>
        public ActionResult DanhSachNVPB()
        {
            try
            {
                StringBuilder buildTree = new StringBuilder();
                var phongBans = linqDM.tbl_DM_PhongBans.ToList();
                buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
                ViewBag.NVPB = buildTree.ToString();
                return View();
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

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

        #region Bindata

        public void buitlTinhThanh(string select)
        {
            IList<Sys_TinhThanh> tinhThanh = linqNS.Sys_TinhThanhs.ToList();

            tinhThanh.Insert(0, new Sys_TinhThanh() { maTinhThanh = "", tenTinhThanh = "[Chọn]" });
            ViewBag.TinhThanh = new SelectList(tinhThanh, "maTinhThanh", "tenTinhThanh", select);
        }
        public void buitlQuocGia(string select)
        {
            IList<Sys_QuocTich> quocGia = linqNS.Sys_QuocTiches.ToList();
            quocGia.Insert(0, new Sys_QuocTich() { maQuocTich = "", tenQuocTich = "[Chọn]" });
            ViewBag.QuocGia = new SelectList(quocGia, "maQuocTich", "tenQuocTich", select);
        }



        #endregion
        public string GetMax()
        {
            return lqPhieuDN.tbl_NS_PhieuCongTacs.OrderByDescending(d => d.ngayLap).Select(t => t.maPhieu).FirstOrDefault();
        }

        public ActionResult ChonPhongBan()
        {
            StringBuilder buildTree = new StringBuilder();
            IList<tbl_DM_PhongBan> phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.PhongBan = buildTree.ToString();
            return PartialView("_ChonPhongBan");
        }

        public ActionResult DanhSachPhongBan()
        {
            StringBuilder buildTree = new StringBuilder();
            IList<tbl_DM_PhongBan> phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.PhongBan = buildTree.ToString();
            return View();
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

        #region Lưu lại vi pham vượt quá số ngày qui định
        public void LuuLaiSoNgayVuotQuaQuiDinh(tbl_NS_PhieuCongTac tblPhieuDeNghi)
        {
            try
            {
                var phieuViPham = lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.Where(d => d.maPhieu == tblPhieuDeNghi.maPhieu).FirstOrDefault();
                if (phieuViPham == null)
                {
                    tbl_NS_PhieuDeNghi_ViPham viPham = new tbl_NS_PhieuDeNghi_ViPham();
                    viPham.maPhieu = tblPhieuDeNghi.maPhieu;
                    viPham.maNhanVien = tblPhieuDeNghi.maNhanVien;
                    viPham.maCongViec = taskIDSystem;
                    viPham.soNgayRangBuoc = linqDM.tbl_DM_LoaiNghiPheps.Where(d => d.maLoaiNghiPhep == "PhieuCongTac").Select(d => d.soNgayRangBuoc).FirstOrDefault() ?? 0;
                    viPham.ngayBatDau = tblPhieuDeNghi.ngayBatDau;
                    viPham.ngayLap = tblPhieuDeNghi.ngayLap;
                    viPham.nguoiLap = GetUser().manv;
                    lqPhieuDN.tbl_NS_PhieuDeNghi_ViPhams.InsertOnSubmit(viPham);
                }
            }
            catch
            {
            }
        }
        #endregion

    }
}
