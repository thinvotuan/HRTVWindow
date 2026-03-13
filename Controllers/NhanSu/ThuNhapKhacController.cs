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
using Worldsoft.Mvc.Web.Util;
using System.Data;

namespace BatDongSan.Controllers.NhanSu
{
    public class ThuNhapKhacController : ApplicationController
    {
        private StringBuilder buildTree = null;
        private IList<tbl_DM_PhongBan> phongBans;
        LinqPhieuDeNghiDataContext lqPhieuDN = new LinqPhieuDeNghiDataContext();
        LinqNhanSuDataContext linqNS = new LinqNhanSuDataContext();
        LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
        tbl_NS_ThuNhapKhac tblPhieuDeNghi;
        IList<tbl_NS_ThuNhapKhac> tblPhieuDeNghis;
        ThuNhapKhac PhieuDeNghiModel;
        public const string taskIDSystem = "ThuNhapKhac";//REGWORKVOTE
        public bool? permission;
        //
        // GET: /PhieuCongTac/

        public ActionResult Index(int? page, string qSearch, int? thang, int? nam, string trangThai)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            BindDataTrangThai(taskIDSystem);
            var userName = GetUser().manv;
            FunThang(0);
            FunNam(DateTime.Now.Year);
            ViewBag.isGet = "True";
            var tblPhieuDeNghis = linqNS.sp_NS_ThuNhapKhac_Index(thang, nam, qSearch, trangThai).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = tblPhieuDeNghis.Count();
            ViewBag.Search = qSearch;
            ViewBag.trangThai = trangThai;
            ViewBag.maNhanVien = GetUser().manv;

            ViewBag.thang = thang;
            ViewBag.nam = nam;
            return View(tblPhieuDeNghis.ToPagedList(currentPageIndex, 50));

        }
        public ActionResult ViewIndex(int? page, string qSearch, int? thang, int? nam, string trangThai)
        {
            var userName = GetUser().manv;

            ViewBag.isGet = "True";
            var tblPhieuDeNghis = linqNS.sp_NS_ThuNhapKhac_Index(thang, nam, qSearch, trangThai).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = tblPhieuDeNghis.Count();
            ViewBag.Search = qSearch;
            ViewBag.trangThai = trangThai;
            ViewBag.maNhanVien = GetUser().manv;
            ViewBag.thang = thang;
            ViewBag.nam = nam;
            return PartialView("ViewIndex", tblPhieuDeNghis.ToPagedList(currentPageIndex, 50));

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

            PhieuDeNghiModel = new ThuNhapKhac();
            PhieuDeNghiModel.ngayLap = DateTime.Now;
            PhieuDeNghiModel.NhanVienLapPhieu = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = GetUser().manv,
                hoVaTen = HoVaTen(GetUser().manv)
            };
            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel();
            PhieuDeNghiModel.soTien = 0;

            buitlLoaiThuNhapKhac(string.Empty);
            builtThang(DateTime.Now.Month);
            builtNam(DateTime.Now.Year);

            return View(PhieuDeNghiModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection coll)
        {
            try
            {
                tblPhieuDeNghi = new tbl_NS_ThuNhapKhac();
                tblPhieuDeNghi.nguoiLap = GetUser().manv;
                tblPhieuDeNghi.ngayLap = DateTime.Now;
                tblPhieuDeNghi.maNhanVien = coll.Get("NhanVien.maNhanVien");

                tblPhieuDeNghi.thang = Convert.ToInt32(coll.Get("thang"));
                tblPhieuDeNghi.nam = Convert.ToInt32(coll.Get("nam"));
                tblPhieuDeNghi.soTien = Convert.ToDouble(coll.Get("soTien"));
                tblPhieuDeNghi.lyDo = coll.Get("lyDo");
                tblPhieuDeNghi.loaiThuNhapKhac = coll.Get("loaiThuNhapKhac");

                linqNS.tbl_NS_ThuNhapKhacs.InsertOnSubmit(tblPhieuDeNghi);
                linqNS.SubmitChanges();
                //Insert chi tiết nhân viên thuộc phòng ban
                string[] maNhanVien = coll.GetValues("maNhanVien");
                List<tbl_NS_ThuNhapKhac_DSNhanVien> chiTiet = new List<tbl_NS_ThuNhapKhac_DSNhanVien>();
                tbl_NS_ThuNhapKhac_DSNhanVien ct;
                if (maNhanVien != null && maNhanVien.Count() > 0)
                {
                    for (int i = 0; i < maNhanVien.Count(); i++)
                    {
                        ct = new tbl_NS_ThuNhapKhac_DSNhanVien();
                        ct.soPhieu = Convert.ToString(tblPhieuDeNghi.id);
                        ct.maNhanVien = coll.GetValues("maNhanVien")[i];
                        ct.lyDoNV = Convert.ToString(coll.GetValues("lyDoNV")[i]);
                        ct.soTien = String.IsNullOrEmpty(coll.GetValues("soTienNV")[i]) ? 0 : Convert.ToDecimal(coll.GetValues("soTienNV")[i]);
                        chiTiet.Add(ct);
                    }
                }

                if (chiTiet != null && chiTiet.Count > 0)
                {
                    lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens.InsertAllOnSubmit(chiTiet);
                }
                lqPhieuDN.SubmitChanges();
                return RedirectToAction("Edit", new { id = tblPhieuDeNghi.id });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
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
                var phieu = linqNS.tbl_NS_ThuNhapKhacs.Where(d => d.id == id).FirstOrDefault();
                linqNS.tbl_NS_ThuNhapKhacs.DeleteOnSubmit(phieu);
                linqNS.SubmitChanges();
                //Delete danh sách nhân viên
                var delNV = lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens.Where(d => d.soPhieu == Convert.ToString(id));
                lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens.DeleteAllOnSubmit(delNV);
                lqPhieuDN.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View("error");
            }
        }
        public ActionResult Edit(int id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var dataPhieuCongTac = linqNS.tbl_NS_ThuNhapKhacs.Where(d => d.id == id).FirstOrDefault();
            PhieuDeNghiModel = new ThuNhapKhac();
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
            PhieuDeNghiModel.id = id;
            PhieuDeNghiModel.NhanVienLapPhieu = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.nguoiLap,
                hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap)
            };

            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.maNhanVien,
                hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien)
            };
            PhieuDeNghiModel.soTien = (decimal?)dataPhieuCongTac.soTien;
            PhieuDeNghiModel.thang = dataPhieuCongTac.thang;
            PhieuDeNghiModel.nam = dataPhieuCongTac.nam;
            PhieuDeNghiModel.lyDo = dataPhieuCongTac.lyDo;

            buitlLoaiThuNhapKhac(dataPhieuCongTac.loaiThuNhapKhac);
            builtNam(dataPhieuCongTac.nam ?? 0);
            builtThang(dataPhieuCongTac.thang ?? 0);
            ViewBag.LoaiThuNhap = dataPhieuCongTac.loaiThuNhapKhac;
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(id.ToString(), PhieuDeNghiModel.maQuiTrinhDuyet);
            LinqHeThongDataContext ht = new LinqHeThongDataContext();
            string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
            ViewBag.HoTen = hoTen;
            int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id.ToString()).Select(d => d.trangThai).FirstOrDefault() ?? 0;
            ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            var maBuocDuyet = ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id.ToString()).FirstOrDefault();
            ViewBag.TenBuocDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == (maBuocDuyet == null ? 0 : maBuocDuyet.trangThai)).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            //Danh sách chi tiêt nhân viên tham dự tăng ca
            ViewBag.ChiTietNhanVien = (from d in lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens
                                       join lq in lqPhieuDN.GetTable<vw_NS_DanhSachNhanVienTheoPhongBan>() on d.maNhanVien equals lq.maNhanVien
                                       //where d.soPhieu == dataPhieuCongTac.soPhieu
                                       where d.soPhieu == Convert.ToString(id)
                                       orderby d.id
                                       select new NhanVienModel
                                       {

                                           maNhanVien = d.maNhanVien,
                                           hoVaTen = HoVaTen(d.maNhanVien),
                                           tenChucDanh = lq.TenChucDanh,
                                           tenPhongBan = lq.tenPhongBan,
                                           email = lq.email,
                                           soTienNV = d.soTien ?? 0,
                                           lyDoNV = d.lyDoNV

                                       }).ToList();
            return View(PhieuDeNghiModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int id, FormCollection coll)
        {
            try
            {
                tblPhieuDeNghi = linqNS.tbl_NS_ThuNhapKhacs.Where(d => d.id == id).FirstOrDefault();
                tblPhieuDeNghi.thang = Convert.ToInt32(coll.Get("thang"));
                tblPhieuDeNghi.nam = Convert.ToInt32(coll.Get("nam"));
                tblPhieuDeNghi.soTien = 0;
                tblPhieuDeNghi.loaiThuNhapKhac = coll.Get("loaiThuNhapKhac");
                tblPhieuDeNghi.lyDo = coll.Get("lyDo");
                linqNS.SubmitChanges();
                //Delete danh sách nhân viên
                var delNV = lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens.Where(d => d.soPhieu == Convert.ToString(id));
                lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens.DeleteAllOnSubmit(delNV);
                lqPhieuDN.SubmitChanges();
                //Insert chi tiết nhân viên thuộc phòng ban
                string[] maNhanVien = coll.GetValues("maNhanVien");
                List<tbl_NS_ThuNhapKhac_DSNhanVien> chiTiet = new List<tbl_NS_ThuNhapKhac_DSNhanVien>();
                tbl_NS_ThuNhapKhac_DSNhanVien ct;
                if (maNhanVien != null && maNhanVien.Count() > 0)
                {
                    for (int i = 0; i < maNhanVien.Count(); i++)
                    {
                        ct = new tbl_NS_ThuNhapKhac_DSNhanVien();
                        ct.soPhieu = Convert.ToString(tblPhieuDeNghi.id);
                        ct.maNhanVien = coll.GetValues("maNhanVien")[i];
                        ct.soTien = String.IsNullOrEmpty(coll.GetValues("soTienNV")[i]) ? 0 : Convert.ToDecimal(coll.GetValues("soTienNV")[i]);
                        ct.lyDoNV = Convert.ToString(coll.GetValues("lyDoNV")[i]);
                        chiTiet.Add(ct);
                    }
                }



                if (chiTiet != null && chiTiet.Count > 0)
                {
                    lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens.InsertAllOnSubmit(chiTiet);
                }
                lqPhieuDN.SubmitChanges();
                return RedirectToAction("Edit", new { id = tblPhieuDeNghi.id });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }


        public ActionResult Details(int id)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var dataPhieuCongTac = linqNS.tbl_NS_ThuNhapKhacs.Where(d => d.id == id).FirstOrDefault();
            PhieuDeNghiModel = new ThuNhapKhac();
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.maQuiTrinhDuyet = dataPhieuCongTac.maQuiTrinhDuyet ?? 0;
            PhieuDeNghiModel.id = id;
            PhieuDeNghiModel.NhanVienLapPhieu = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.nguoiLap,
                hoVaTen = HoVaTen(dataPhieuCongTac.nguoiLap)
            };

            PhieuDeNghiModel.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = dataPhieuCongTac.maNhanVien,
                hoVaTen = HoVaTen(dataPhieuCongTac.maNhanVien)
            };
            PhieuDeNghiModel.thang = dataPhieuCongTac.thang;
            PhieuDeNghiModel.nam = dataPhieuCongTac.nam;
            PhieuDeNghiModel.ngayLap = dataPhieuCongTac.ngayLap;
            PhieuDeNghiModel.lyDo = dataPhieuCongTac.lyDo;
            PhieuDeNghiModel.soTien = Convert.ToDecimal(dataPhieuCongTac.soTien);
            ViewBag.TenLoaiThuNhap = linqDM.tbl_DM_ThuNhapKhacs.Where(d => d.maLoaiThuNhapKhac == dataPhieuCongTac.loaiThuNhapKhac).Select(d => d.tenLoaiThuNhapKhac).FirstOrDefault();
            DMNguoiDuyetController nd = new DMNguoiDuyetController();
            PhieuDeNghiModel.Duyet = nd.GetDetailByMaPhieuTheoQuiTrinhDong(id.ToString(), PhieuDeNghiModel.maQuiTrinhDuyet);
            LinqHeThongDataContext ht = new LinqHeThongDataContext();
            string hoTen = (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ho).FirstOrDefault() ?? string.Empty) + " " + (ht.GetTable<tbl_NS_NhanVien>().Where(d => d.maNhanVien == GetUser().manv).Select(d => d.ten).FirstOrDefault() ?? string.Empty);
            ViewBag.HoTen = hoTen;
            int trangThaiHT = (int?)ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id.ToString()).Select(d => d.trangThai).FirstOrDefault() ?? 0;
            ViewBag.TenTrangThaiDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == trangThaiHT).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            ViewBag.URL = Request.Url.AbsoluteUri.ToString();
            var maBuocDuyet = ht.tbl_HT_DMNguoiDuyets.OrderByDescending(d => d.ID).Where(d => d.maPhieu == id.ToString()).FirstOrDefault();
            ViewBag.TenBuocDuyet = ht.tbl_HT_QuiTrinhDuyet_BuocDuyets.Where(d => d.id == (maBuocDuyet == null ? 0 : maBuocDuyet.trangThai)).Select(d => d.maBuocDuyet).FirstOrDefault() ?? string.Empty;
            //Danh sách chi tiêt nhân viên tham dự tăng ca
            ViewBag.ChiTietNhanVien = (from d in lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens
                                       join lq in lqPhieuDN.GetTable<vw_NS_DanhSachNhanVienTheoPhongBan>() on d.maNhanVien equals lq.maNhanVien
                                       //where d.soPhieu == dataPhieuCongTac.soPhieu
                                       where d.soPhieu == Convert.ToString(id)
                                       orderby d.id
                                       select new NhanVienModel
                                       {

                                           maNhanVien = d.maNhanVien,
                                           hoVaTen = HoVaTen(d.maNhanVien),
                                           tenChucDanh = lq.TenChucDanh,
                                           tenPhongBan = lq.tenPhongBan,
                                           email = lq.email,
                                           soTienNV = d.soTien ?? 0,
                                           lyDoNV = d.lyDoNV
                                       }).ToList();
            return View(PhieuDeNghiModel);
        }
        private void FunThang(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 0; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["lstThang"] = new SelectList(dics, "Key", "Value", value);
        }
        private void FunNam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 2015; i < 2031; i++)
            {
                dics[i] = i.ToString();
            }
            ViewData["lstNam"] = new SelectList(dics, "Key", "Value", value);
        }
        public FileResult DownloadImportFile()
        {
            string savedFileName = Path.Combine("/UploadFiles/Template/", "PhieuNhapThuNhapKhac.xlsx");
            return File(savedFileName, "multipart/form-data", "PhieuNhapThuNhapKhac.xlsx");
        }
        public ActionResult ImportExcelData(string excelPath)
        {
            try
            {
                string[] supportedFiles = { ".xlsx", ".xls" };
                HttpPostedFileBase File;
                File = Request.Files[0];
                int flashImportTrung = 0;
                if (File.ContentLength > 0)
                {
                    string extension = Path.GetExtension(File.FileName);
                    bool exist = Array.Exists(supportedFiles, element => element == extension);
                    if (exist == false)
                    {
                        return Json(new { success = false });
                    }
                    else
                    {
                        var date = DateTime.Now.ToString("yyyyMMdd-HHMMss");
                        string savedLocation = "/UploadFiles/ThuNhapKhac/";
                        Directory.CreateDirectory(savedLocation);
                        var filePath = Server.MapPath(savedLocation);
                        string fileName = date.ToString() + File.FileName;
                        string savedFileName = Path.Combine(filePath, fileName);
                        File.SaveAs(savedFileName);

                        ExcelDataProcessing excelDataProcessor = new ExcelDataProcessing(savedFileName);
                        DataTable dt = excelDataProcessor.GetDataTableWorkSheet("PhieuThuNhapKhac");
                        var HSMTBanDau = "";
                        var ThangBanDau = "";
                        var flashBanDau = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            if (flashBanDau == 0)
                            {
                                var maLoaiTNKTemp = row["Mã loại thu nhập"].ToString();
                                var thangBanDauTemp = row["Tháng"].ToString();
                                HSMTBanDau = maLoaiTNKTemp;
                                ThangBanDau = thangBanDauTemp;
                                flashBanDau = 1;
                                break;
                            }

                        }
                        var thuTuParent = 0;
                        var Flash = 0;
                        var maPhieuTNK = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            var maLoaiTNK = row["Mã loại thu nhập"].ToString();
                            var ThangTNK = row["Tháng"].ToString();
                            if (maLoaiTNK == null || maLoaiTNK == "")
                            {
                                return Json(new { success = true });
                            }
                            if (HSMTBanDau == maLoaiTNK && ThangBanDau == ThangTNK)
                            {
                                Flash = 0;
                            }
                            else
                            {
                                HSMTBanDau = maLoaiTNK;
                                ThangBanDau = ThangTNK;
                                Flash = 1;
                            }
                            if (Flash == 1 || thuTuParent == 0)
                            {

                                thuTuParent = 1;
                                Flash = 0;
                                maPhieuTNK = 0;
                                tbl_NS_ThuNhapKhac thuNhapKhacImport = new tbl_NS_ThuNhapKhac();
                                thuNhapKhacImport.maNhanVien = null;
                                thuNhapKhacImport.nguoiLap = GetUser().manv;
                                thuNhapKhacImport.ngayLap = DateTime.Now;
                                thuNhapKhacImport.loaiThuNhapKhac = row["Mã loại thu nhập"].ToString();
                                thuNhapKhacImport.nam = String.IsNullOrEmpty(row["Năm"].ToString()) ? 0 : Convert.ToInt32(row["Năm"]);
                                thuNhapKhacImport.thang = String.IsNullOrEmpty(row["Tháng"].ToString()) ? 0 : Convert.ToInt32(row["Tháng"]);
                                linqNS.tbl_NS_ThuNhapKhacs.InsertOnSubmit(thuNhapKhacImport);
                                linqNS.SubmitChanges();
                                maPhieuTNK = thuNhapKhacImport.id;
                            }
                            if (Flash == 0)
                            {
                                var tenNhanVien = row["Tên nhân viên"].ToString().Trim();

                                var maNhanVien = linqNS.tbl_NS_NhanViens.Where(d => d.ho.Trim() + " " + d.ten.Trim() == row["Tên nhân viên"].ToString().Trim() && d.trangThai == 0).Select(d => d.maNhanVien).FirstOrDefault();
                                if (maNhanVien != null && maNhanVien != "")
                                {
                                    var thang = Convert.ToInt32(row["Tháng"]);
                                    var nam = Convert.ToInt32(row["Năm"]);


                                    //Check if ma nhan vien
                                    var checkMaNV = (from p in lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens
                                                     join q in lqPhieuDN.GetTable<tbl_NS_ThuNhapKhac>() on p.soPhieu equals q.id.ToString()
                                                     where (p.maNhanVien == maNhanVien && q.thang == thang && q.nam == nam && q.loaiThuNhapKhac == maLoaiTNK)
                                                     select new ThuNhapKhac
                                                     {
                                                         id = q.id,
                                                         maNhanVien = p.maNhanVien
                                                     }).FirstOrDefault();
                                    if (checkMaNV == null)
                                    {

                                        var idThuNhapKhac = lqPhieuDN.GetTable<tbl_NS_ThuNhapKhac>().OrderByDescending(d => d.id).Select(d => d.id).FirstOrDefault();
                                        // Import chi tiet
                                        tbl_NS_ThuNhapKhac_DSNhanVien thuNhapKhacImportNhanVien = new tbl_NS_ThuNhapKhac_DSNhanVien();
                                        thuNhapKhacImportNhanVien.maNhanVien = maNhanVien;
                                        thuNhapKhacImportNhanVien.soPhieu = Convert.ToString(idThuNhapKhac);
                                        thuNhapKhacImportNhanVien.soTien = String.IsNullOrEmpty(row["Số tiền"].ToString()) ? 0 : Convert.ToDecimal(row["Số tiền"]);
                                        thuNhapKhacImportNhanVien.lyDoNV = row["Lý do"].ToString();
                                        lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens.InsertOnSubmit(thuNhapKhacImportNhanVien);
                                        lqPhieuDN.SubmitChanges();
                                    }
                                    else
                                    {
                                        // Luu nhan vien khong import duoc
                                        sys_NhanVienImportFailed tblImportFailed = new sys_NhanVienImportFailed();
                                        tblImportFailed.tenNhanVien = row["Tên nhân viên"].ToString().Trim();
                                        tblImportFailed.lyDoFaild = "Import thu nhập khác không được. Nhân viên này đã được import trước đó. Mã phiếu tham chiếu: " + checkMaNV.id;
                                        tblImportFailed.ngayLap = DateTime.Now;
                                        tblImportFailed.nguoiLap = GetUser().manv;
                                        tblImportFailed.thang = Convert.ToInt32(row["Tháng"]);
                                        tblImportFailed.nam = Convert.ToInt32(row["Năm"]);
                                        tblImportFailed.soTien = String.IsNullOrEmpty(row["Số tiền"].ToString()) ? 0 : Convert.ToDecimal(row["Số tiền"]);
                                        tblImportFailed.loaiImport = row["Mã loại thu nhập"].ToString();
                                        tblImportFailed.tenCongViec = taskIDSystem;
                                        tblImportFailed.maPhieuThamChieu = String.IsNullOrEmpty(maPhieuTNK.ToString()) ? string.Empty : Convert.ToString(maPhieuTNK);
                                        linqNS.sys_NhanVienImportFaileds.InsertOnSubmit(tblImportFailed);
                                        linqNS.SubmitChanges();
                                        //
                                        flashImportTrung = 1;
                                        // Cap nhat lap lai cho phieu cha
                                        var LapLaiTNK = linqNS.tbl_NS_ThuNhapKhacs.Where(d => d.id == maPhieuTNK).FirstOrDefault();
                                        LapLaiTNK.LapLai = 1;
                                        linqNS.SubmitChanges();
                                    }
                                }
                                else
                                {
                                    // Luu nhan vien khong import duoc
                                    sys_NhanVienImportFailed tblImportFailed = new sys_NhanVienImportFailed();
                                    tblImportFailed.tenNhanVien = row["Tên nhân viên"].ToString().Trim();
                                    tblImportFailed.lyDoFaild = "Import thu nhập khác không được. Không tìm thấy tên nhân viên, hoặc nhân viên đã nghỉ việc.";
                                    tblImportFailed.ngayLap = DateTime.Now;
                                    tblImportFailed.nguoiLap = GetUser().manv;
                                    tblImportFailed.thang = Convert.ToInt32(row["Tháng"]);
                                    tblImportFailed.nam = Convert.ToInt32(row["Năm"]);
                                    tblImportFailed.soTien = String.IsNullOrEmpty(row["Số tiền"].ToString()) ? 0 : Convert.ToDecimal(row["Số tiền"]);

                                    tblImportFailed.loaiImport = row["Mã loại thu nhập"].ToString();
                                    tblImportFailed.tenCongViec = taskIDSystem;
                                    tblImportFailed.maPhieuThamChieu = String.IsNullOrEmpty(maPhieuTNK.ToString()) ? string.Empty : Convert.ToString(maPhieuTNK);
                                    linqNS.sys_NhanVienImportFaileds.InsertOnSubmit(tblImportFailed);
                                    linqNS.SubmitChanges();
                                    //
                                    flashImportTrung = 1;

                                    // Cap nhat lap lai cho phieu cha
                                    var LapLaiTNK = linqNS.tbl_NS_ThuNhapKhacs.Where(d => d.id == maPhieuTNK).FirstOrDefault();
                                    LapLaiTNK.LapLai = 1;
                                    linqNS.SubmitChanges();
                                }
                            }


                        }

                        System.IO.File.Delete(Server.MapPath("/UploadFiles/ThuNhapKhac/" + fileName));
                    }
                }
                SaveActiveHistory("Import danh sách phiếu thu nhập khác.");
                return Json(new { success = true, flashImportTrung = flashImportTrung });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, flashImportTrung = ex.Message });
            }
        }

        public ActionResult ImportExcelDataNhanVien(string excelPath, string soPhieu)
        {
            try
            {
                string[] supportedFiles = { ".xlsx", ".xls" };
                HttpPostedFileBase File;
                File = Request.Files[0];

                if (File.ContentLength > 0)
                {
                    string extension = Path.GetExtension(File.FileName);
                    bool exist = Array.Exists(supportedFiles, element => element == extension);

                    if (exist == false)
                    {
                        return Json(new { success = false });
                    }
                    else
                    {
                        var date = DateTime.Now.ToString("yyyyMMdd-HHMMss");
                        string savedLocation = "/UploadFiles/ThuNhapKhac/";
                        Directory.CreateDirectory(savedLocation);
                        var filePath = Server.MapPath(savedLocation);
                        string fileName = date.ToString() + File.FileName;
                        string savedFileName = Path.Combine(filePath, fileName);

                        File.SaveAs(savedFileName);

                        ExcelDataProcessing excelDataProcessor = new ExcelDataProcessing(savedFileName);
                        DataTable dt = excelDataProcessor.GetDataTableWorkSheet("danhsachthunhapkhac");

                        List<tbl_NS_ThuNhapKhac_DSNhanVien> lstNhanVien = new List<tbl_NS_ThuNhapKhac_DSNhanVien>();

                        foreach (DataRow row in dt.Rows)
                        {
                            var ct = new tbl_NS_ThuNhapKhac_DSNhanVien();
                            ct.soPhieu = soPhieu;

                            if (row["Mã nhân viên"] != null && !string.IsNullOrEmpty(row["Mã nhân viên"].ToString()))
                            {
                                ct.maNhanVien = row["Mã nhân viên"].ToString();
                            }
                            else
                            {
                                ct.maNhanVien = linqNS.tbl_NS_NhanViens.Where(d => d.ho + " " + d.ten 
                                    == (row["Họ và tên"] != null ? row["Họ và tên"].ToString() : string.Empty)).Select(d => d.maNhanVien).FirstOrDefault() ?? string.Empty;
                            }
                            //trường hợp không có mã nhân viên thì thoát vòng lặp
                            if (string.IsNullOrEmpty(ct.maNhanVien))
                            {
                                continue;
                            }
                            else
                            {
                                //Đã tồn tại
                                if (lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens.Where(d => d.soPhieu == soPhieu && d.maNhanVien == ct.maNhanVien).Count() > 0)
                                {
                                    continue;
                                }
                                else
                                {
                                    ct.soTien = Convert.ToDecimal(!string.IsNullOrEmpty(row["Số tiền"].ToString()) ? row["Số tiền"].ToString() : "0");
                                    ct.lyDoNV = (row["Lý do"] != null ? row["Lý do"].ToString() : string.Empty);

                                    lstNhanVien.Add(ct);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(soPhieu) && lstNhanVien != null && lstNhanVien.Count() > 0)
                        {
                            lqPhieuDN.tbl_NS_ThuNhapKhac_DSNhanViens.InsertAllOnSubmit(lstNhanVien);
                            lqPhieuDN.SubmitChanges();
                        }

                        System.IO.File.Delete(Server.MapPath("/UploadFiles/ThuNhapKhac/" + fileName));
                    }
                }

                SaveActiveHistory("Import danh sách phiếu thu nhập khác.");
                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                //Log4Net.WriteLog(log4net.Core.Level.Error, "lỗi xảy ra:" + ex.Message);
                return Json(new { success = false, messageError =ex.Message });
            }
        }

        #region Xuất excel
        public void XuatFile(string qSearch, int thang, int nam, string trangThai)
        {

            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplateThuNhapKhac.xls"), FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);
            filename += "DanhSachThuNhapKhac.xls";


            var sheet = workbook.GetSheet("danhsachthunhapkhac");

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


            Row rowC = null;
            //Khai báo row đầu tiên
            int firstRowNumber = 1;

            string rowtitle = "DANH SÁCH THU NHẬP KHÁC";
            var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 3, rowtitle.ToUpper());
            titleCell.CellStyle = styleTitle;

            ++firstRowNumber;

            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Mã nhân viên");
            list1.Add("Tên nhân viên");
            list1.Add("Tháng");
            list1.Add("Năm");
            list1.Add("Loại thu nhập");
            list1.Add("Số tiền");

            //Start row 13
            var headerRow = sheet.CreateRow(2);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end
            var idRowStart = 3;
            var listDS = (from p in linqNS.GetTable<tbl_NS_ThuNhapKhac_DSNhanVien>()
                          join q in linqNS.tbl_NS_ThuNhapKhacs on p.soPhieu equals Convert.ToString(q.id)
                          where q.thang == thang && q.nam == nam
                          select new ThuNhapKhac
                          {
                              maNhanVien = p.maNhanVien,
                              thang = q.thang,
                              nam = q.nam,
                              soTien = p.soTien,
                              lyDo = p.lyDoNV,
                              maLoaiThuNhapKhac = q.loaiThuNhapKhac,

                          }).OrderBy(p => p.maNhanVien).ToList();
            // var datas = linqNS.sp_NS_ThuNhapKhac_Index(thang, nam, qSearch, trangThai).OrderBy(d=>d.maNhanVien).ToList(); ;
            //#region
            if (listDS != null && listDS.Count > 0)
            {
                var stt = 0;
                int dem = 0;
                int demChild = 0;
                int sttChild = 0;
                //Giai đoạn
                foreach (var itemChild in listDS)
                {
                    demChild = 0;
                    sttChild++;
                    idRowStart++;
                    rowC = sheet.CreateRow(idRowStart);
                    var tenNV = HoVaTen(itemChild.maNhanVien);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, sttChild.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, itemChild.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, tenNV.ToString(), hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, Convert.ToString(itemChild.thang ?? 0), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, Convert.ToString(itemChild.nam ?? 0), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, itemChild.maLoaiThuNhapKhac, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, String.Format("{0:#,##0.###}", itemChild.soTien), hStyleConRight);

                }
                sttChild = 0;
                // End for child


                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 210);
                sheet.SetColumnWidth(4, 20 * 210);
                sheet.SetColumnWidth(5, 20 * 270);
                sheet.SetColumnWidth(6, 20 * 270);
            }
            else
            {

                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 210);
                sheet.SetColumnWidth(4, 20 * 210);
                sheet.SetColumnWidth(5, 20 * 270);
                sheet.SetColumnWidth(6, 20 * 270);

            }

            var stream = new MemoryStream();
            workbook.Write(stream);

            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
            Response.Clear();

            Response.BinaryWrite(stream.GetBuffer());
            Response.End();

        }

        public void XuatFileImportNhanVien()
        {

            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplateThuNhapKhac.xls"), FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);
            filename += "DanhSachThuNhapKhac.xls";


            var sheet = workbook.GetSheet("danhsachthunhapkhac");

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


            Row rowC = null;


            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Mã nhân viên");
            list1.Add("Họ và tên");
            list1.Add("Số tiền");
            list1.Add("Lý do");

            //Start row 13
            var headerRow = sheet.CreateRow(0);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end
            var idRowStart = 0;
            var listDS = (from p in linqNS.GetTable<tbl_NS_NhanVien>()
                          where (p.trangThai ?? 0) == 0
                          select new ThuNhapKhac
                         {

                             NhanVien = new NhanVienModel
                             {
                                 maNhanVien = p.maNhanVien,
                                 hoVaTen = p.ho + " " + p.ten
                             },
                             soTien = 0,
                         }).OrderBy(p => p.NhanVien.hoVaTen).ToList();
            // var datas = linqNS.sp_NS_ThuNhapKhac_Index(thang, nam, qSearch, trangThai).OrderBy(d=>d.maNhanVien).ToList(); ;
            //#region
            if (listDS != null && listDS.Count > 0)
            {
                int demChild = 0;
                int sttChild = 0;
                //Giai đoạn
                foreach (var itemChild in listDS)
                {
                    demChild = 0;
                    sttChild++;
                    ++idRowStart;
                    rowC = sheet.CreateRow(idRowStart);

                    ReportHelperExcel.SetAlignment(rowC, demChild++, sttChild.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, itemChild.NhanVien.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, itemChild.NhanVien.hoVaTen, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, String.Format("{0:#,##0.###}", 0), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, string.Empty, hStyleConLeft);
                }
                sttChild = 0;
                // End for child
                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 210);
                sheet.SetColumnWidth(4, 20 * 300);
            }
            else
            {
                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 210);
                sheet.SetColumnWidth(4, 20 * 300);
            }

            var stream = new MemoryStream();
            workbook.Write(stream);

            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
            Response.Clear();

            Response.BinaryWrite(stream.GetBuffer());
            Response.End();

        }
        #endregion
        #region
        public void XuatFileImportTrung(string maPhieu)
        {
            var filename = "";
            var virtualPath = HttpRuntime.AppDomainAppVirtualPath;
            var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplateThuNhapKhac.xls"), FileMode.Open, FileAccess.Read);

            var workbook = new HSSFWorkbook(fileStream, true);
            filename += "DanhSachTNKKhongImportDuoc.xls";


            var sheet = workbook.GetSheet("danhsachthunhapkhac");

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


            Row rowC = null;
            //Khai báo row đầu tiên
            int firstRowNumber = 1;

            string rowtitle = "DANH SÁCH THU NHẬP KHÁC KHÔNG IMPORT ĐƯỢC (Ứng với số phiếu: " + maPhieu + ")";
            var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 3, rowtitle.ToUpper());
            titleCell.CellStyle = styleTitle;

            ++firstRowNumber;

            var list1 = new List<string>();
            list1.Add("STT");
            list1.Add("Mã nhân viên");
            list1.Add("Tên nhân viên");
            list1.Add("Tháng");
            list1.Add("Năm");
            list1.Add("Loại thu nhập");
            list1.Add("Số tiền");
            list1.Add("Lý do failed");
            //Start row 13
            var headerRow = sheet.CreateRow(2);
            ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);

            //Create header end
            var idRowStart = 3;
            var listDS = linqNS.sys_NhanVienImportFaileds.Where(d => d.maPhieuThamChieu == maPhieu && d.tenCongViec == taskIDSystem).ToList();
            //#region
            if (listDS != null && listDS.Count > 0)
            {
                var stt = 0;
                int dem = 0;
                int demChild = 0;
                int sttChild = 0;
                //Giai đoạn
                foreach (var itemChild in listDS)
                {
                    demChild = 0;
                    sttChild++;
                    idRowStart++;
                    rowC = sheet.CreateRow(idRowStart);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, sttChild.ToString(), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, itemChild.maNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, itemChild.tenNhanVien, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, Convert.ToString(itemChild.thang ?? 0), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, Convert.ToString(itemChild.nam ?? 0), hStyleConCenter);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, itemChild.loaiImport, hStyleConLeft);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, String.Format("{0:#,##0.###}", itemChild.soTien), hStyleConRight);
                    ReportHelperExcel.SetAlignment(rowC, demChild++, itemChild.lyDoFaild, hStyleConLeft);
                }
                sttChild = 0;
                // End for child


                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 210);
                sheet.SetColumnWidth(4, 20 * 210);
                sheet.SetColumnWidth(5, 20 * 270);
                sheet.SetColumnWidth(6, 20 * 270);
                sheet.SetColumnWidth(7, 50 * 270);
            }
            else
            {

                sheet.SetColumnWidth(0, 5 * 250);
                sheet.SetColumnWidth(1, 20 * 270);
                sheet.SetColumnWidth(2, 20 * 270);
                sheet.SetColumnWidth(3, 20 * 210);
                sheet.SetColumnWidth(4, 20 * 210);
                sheet.SetColumnWidth(5, 20 * 270);
                sheet.SetColumnWidth(6, 20 * 270);
                sheet.SetColumnWidth(7, 50 * 270);

            }

            var stream = new MemoryStream();
            workbook.Write(stream);

            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", filename));
            Response.Clear();

            Response.BinaryWrite(stream.GetBuffer());
            Response.End();
        }
        #endregion
        public ActionResult ChonNhanVien()
        {
            StringBuilder buildTree = new StringBuilder();
            var phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.NVPB = buildTree.ToString();
            return PartialView("_ChonNhanVien");
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


        public void buitlLoaiThuNhapKhac(string select)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var loaiThuNhapKhac = linqDM.tbl_DM_ThuNhapKhacs.ToList();

            dict.Add("", "[Chọn loại thu nhập]");
            foreach (var item in loaiThuNhapKhac)
            {
                dict.Add(item.maLoaiThuNhapKhac.ToString(), item.tenLoaiThuNhapKhac);
            }
            ViewBag.loaiThuNhapKhac = new SelectList(dict, "Key", "Value", select);

        }
        public void builtThang(int thang)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = 1; i <= 12; i++)
            {
                dict.Add(i, i);
            }
            ViewBag.Thangs = new SelectList(dict, "Key", "Value", thang);
        }
        public void builtNam(int nam)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = DateTime.Now.Year - 5; i <= DateTime.Now.Year + 5; i++)
            {
                dict.Add(i, i);
            }
            ViewBag.Nams = new SelectList(dict, "Key", "Value", nam);
        }
        public string HoVaTen(string MaNV)
        {

            return linqNS.tbl_NS_NhanViens.Where(d => d.maNhanVien == MaNV).Select(d => d.ho + " " + d.ten).FirstOrDefault() ?? string.Empty;
        }
        public ActionResult ChonPhongBan()
        {
            StringBuilder buildTree = new StringBuilder();
            IList<tbl_DM_PhongBan> phongBans = linqDM.tbl_DM_PhongBans.ToList();
            buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
            ViewBag.PhongBan = buildTree.ToString();
            return PartialView("_ChonPhongBan");
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
        #region Cây sơ đồ tổ chức nhân viên và phòng ban

        //Load danh sach nhan vien khi click button add them:
        public ActionResult DanhSachNhanVien()
        {
            try
            {
                buildTree = new StringBuilder();
                phongBans = linqDM.tbl_DM_PhongBans.ToList();
                buildTree = TreePhongBans.BuildTreeDepartment(phongBans);
                ViewBag.departments = buildTree.ToString();
                ViewBag.shiftType = linqDM.tbl_NS_PhanCas.Where(t => t.tenPhanCa != "").ToList();
                ViewBag.page = 0;
                ViewBag.total = 0;
                return View(phongBans);
            }
            catch (Exception ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
        }
        public ActionResult LoadNhanVienTNK(string qSearch, int _page, string parrentId)
        {
            try
            {

                string parentID = linqDM.tbl_DM_PhongBans.Where(d => d.maPhongBan == parrentId).Select(d => d.maPhongBan).FirstOrDefault() ?? string.Empty;
                if (String.IsNullOrEmpty(parentID))
                {
                    parrentId = string.Empty;
                }
                int page = _page == 0 ? 1 : _page;
                int pIndex = page;
                int total = linqDM.sp_PB_DanhSachNhanVien(qSearch, parrentId).Count();
                PagingLoaderController("/ThuNhapKhac/Index/", total, page, "?qsearch=" + qSearch + "&parrentId=" + parrentId);
                ViewBag.nhanVien = linqDM.sp_PB_DanhSachNhanVien(qSearch, parrentId).Skip(start).Take(offset).ToList();
                ViewBag.parrentId = parrentId;
                ViewBag.qSearch = qSearch ?? string.Empty;
                ViewBag.currentMaNV = GetUser().manv;
                return PartialView("LoadNhanVien");
            }
            catch (Exception e)
            {
                ViewData["Message"] = e.Message;
                return View("Error");
            }
        }


        #endregion
    }
}
