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
using System.Data;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel.Contrib;
using Worldsoft.Mvc.Web.Util;
using NPOI.SS.Util;

namespace BatDongSan.Controllers.NhanSu
{
    public class TiLeThanhTichController : ApplicationController
    {
        private StringBuilder buildTree = null;
        private IList<tbl_DM_PhongBan> phongBans;
        LinqPhieuDeNghiDataContext lqPhieuDN = new LinqPhieuDeNghiDataContext();
        LinqNhanSuDataContext linqNS = new LinqNhanSuDataContext();
        LinqDanhMucDataContext linqDM = new LinqDanhMucDataContext();
        //tbl_NS_TiLeThanhTich tblPhieuTiLe;        
        //IList<tbl_NS_TiLeThanhTich> tiLeThanhTichs;
        tbl_NS_TiLeThanhTich phieuTL;
        PhieuDeNghiCongTac PhieuDeNghiModel;
        TiLeThanhTich tiLeThanhTich;
        public const string taskIDSystem = "TiLeThanhTich";
        public bool? permission;
        //
        // GET: /PhieuCongTac/

        public ActionResult Index(int? page, string searchString, int? nam, int? quy)
        {
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            var userName = GetUser().manv;

            ViewBag.isGet = "True";
            var tblPhieuTiLes = linqNS.sp_NS_TiLeThanhTich_Index(quy, nam, searchString, userName).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = tblPhieuTiLes.Count();
            ViewBag.Search = searchString;


            FunQuy(quy ?? 0);
            FunNam(nam ?? 0);
            return View(tblPhieuTiLes.ToPagedList(currentPageIndex, 50));

        }
        public ActionResult ViewIndex(int? page, string searchString, int? nam, int? quy)
        {
            var userName = GetUser().manv;

            ViewBag.isGet = "True";
            var tblPhieuTiLes = linqNS.sp_NS_TiLeThanhTich_Index(quy, nam, searchString, userName).ToList();
            int currentPageIndex = page.HasValue ? page.Value : 1;
            ViewBag.Count = tblPhieuTiLes.Count();
            ViewBag.Search = searchString;

            FunQuy(quy ?? 0);
            FunNam(nam ?? 0);
            return PartialView("ViewIndex", tblPhieuTiLes.ToPagedList(currentPageIndex, 50));

        }

        public ActionResult Create()
        {
            ViewBag.tiLeThanhTich = 1;
            #region Role user
            permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                tiLeThanhTich = new TiLeThanhTich();
                tiLeThanhTich.maPhieu = GenerateUtil.CheckLetter("TLTT", GetMax());
                tiLeThanhTich.ngayLap = DateTime.Now;
                tiLeThanhTich.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
                {
                    maNhanVien = GetUser().manv,
                    hoVaTen = HoVaTen(GetUser().manv)
                };

                tiLeThanhTich.maPhongBan = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == GetUser().manv).Select(d => d.maPhongBan).FirstOrDefault() ?? string.Empty;
                tiLeThanhTich.tenPhongBan = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == GetUser().manv).Select(d => d.tenPhongBan).FirstOrDefault() ?? string.Empty;
                tiLeThanhTich.ngayLap = DateTime.Now;
                FunQuy(1);
                FunNam(DateTime.Now.Year);
                return View(tiLeThanhTich);
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
            ViewBag.tiLeThanhTich = 1;
            try
            {

                phieuTL = new tbl_NS_TiLeThanhTich();
                BindDataToSave(coll, true);
                int kqCheck = CheckQuyNam(phieuTL.quy, phieuTL.nam);
                if (kqCheck == 1)
                {
                    return View("error");
                }
                linqNS.tbl_NS_TiLeThanhTiches.InsertOnSubmit(phieuTL);
                linqNS.SubmitChanges();
                return RedirectToAction("Edit", new { id = phieuTL.soPhieu });
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

                var lstPhieu = linqNS.tbl_NS_TiLeThanhTiches.Where(d => d.soPhieu == id).FirstOrDefault();
                if (lstPhieu.xacNhan == true)
                {
                    return RedirectToAction("index");

                }
                var phieu = linqNS.tbl_NS_TiLeThanhTiches.Where(d => d.soPhieu == id);

                linqNS.tbl_NS_TiLeThanhTiches.DeleteAllOnSubmit(phieu);

                var delChiTietNV = linqNS.tbl_NS_TiLeThanhTich_DSNhanViens.Where(d => d.soPhieu == id);
                linqNS.tbl_NS_TiLeThanhTich_DSNhanViens.DeleteAllOnSubmit(delChiTietNV);
                linqNS.SubmitChanges();


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
            ViewBag.tiLeThanhTich = 1;
            try
            {
                #region Role user
                permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenSua);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion


                GetThongTinTiLeThanhTich(id);
                if (tiLeThanhTich.xacNhan == true)
                {
                    return RedirectToAction("index");
                }
                FunQuy(tiLeThanhTich.quy);
                FunNam(tiLeThanhTich.nam);
                return View(tiLeThanhTich);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection coll)
        {
            ViewBag.tiLeThanhTich = 1;
            try
            {
                FunQuy(1);
                FunNam(DateTime.Now.Year);
                phieuTL = new tbl_NS_TiLeThanhTich();
                BindDataToSave(coll, false);

                linqNS.SubmitChanges();
                return RedirectToAction("Edit", new { id = phieuTL.soPhieu });
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }

        public ActionResult Details(string id)
        {
            ViewBag.tiLeThanhTich = 1;
            try
            {
                #region Role user
                permission = GetPermission(taskIDSystem, BangPhanQuyen.QuyenXem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion


                GetThongTinTiLeThanhTich(id);

                FunQuy(tiLeThanhTich.quy);
                FunNam(tiLeThanhTich.nam);
                return View(tiLeThanhTich);
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("error");
            }
        }




        /// <summary>
        /// Hàm get max tỉ lệ thành tích
        /// </summary>
        /// <returns></returns>
        public string GetMax()
        {
            return linqNS.tbl_NS_TiLeThanhTiches.OrderByDescending(d => d.ngayLap).Select(d => d.soPhieu).FirstOrDefault() ?? String.Empty;
        }
        private void FunQuy(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = 1; i < 13; i++)
            {
                dics[i] = i.ToString();
            }
            var listQuy = dics.ToList();
            listQuy.Insert(0, new KeyValuePair<int, string>(0, "[Chọn]"));
            ViewData["Quys"] = new SelectList(listQuy, "Key", "Value", value);
        }
        private void FunNam(int value)
        {
            Dictionary<int, string> dics = new Dictionary<int, string>();
            for (int i = (DateTime.Now.Year - 5); i < (DateTime.Now.Year + 5); i++)
            {
                dics[i] = i.ToString();
            }
            var listNam = dics.ToList();
            listNam.Insert(0, new KeyValuePair<int, string>(0, "[Chọn]"));
            ViewData["Nams"] = new SelectList(listNam, "Key", "Value", value);
        }
        /// <summary>
        /// Lấy họ và tên nhân viên
        /// </summary>
        /// <param name="MaNV"></param>
        /// <returns></returns>
        public string HoVaTen(string MaNV)
        {

            return linqNS.tbl_NS_NhanViens.Where(d => d.maNhanVien == MaNV).Select(d => d.ho + " " + d.ten).FirstOrDefault();
        }
        public decimal TiLeThue(string MaNV)
        {


            decimal ThanhTich = 0;
            var tenNhanVien = HoVaTen(MaNV);
            var checkNV = linqNS.tbl_NS_TiLeThanhTich_LuongThanhTiches.Where(d => d.tenNhanVien == tenNhanVien).FirstOrDefault();
            if (checkNV != null)
            {
                ThanhTich = checkNV.luongThanhTich ?? 0;
            }
            else
            {
                var hopDongLD = (from p in linqNS.tbl_NS_HopDongLaoDongs
                                 join q in linqNS.GetTable<tbl_HT_DMNguoiDuyet>() on p.soHopDong equals q.maPhieu
                                 where (q.trangThai == 2 && p.maNhanVien == MaNV)
                                 select new
                                 {
                                     maHopDong = p.soHopDong,
                                     luongThanhTich = p.luongThanhTich
                                 }).OrderByDescending(p => p.maHopDong).FirstOrDefault();

                if (hopDongLD != null)
                {
                    // Check ton tai phu luc hop dong lao dong
                    var phuLucHD = (from p in linqNS.tbl_NS_PhuLucHopDongs
                                    join q in linqNS.GetTable<tbl_HT_DMNguoiDuyet>() on p.soPhuLuc equals q.maPhieu
                                    where (q.trangThai == 2 && p.soHopDong == hopDongLD.maHopDong)
                                    select new
                                    {
                                        maPhuLucHD = p.soPhuLuc,
                                        luongThanhTichPL = p.luongThanhTich
                                    }).OrderByDescending(p => p.maPhuLucHD).FirstOrDefault();
                    if (phuLucHD != null)
                    {
                        ThanhTich = phuLucHD.luongThanhTichPL ?? 0;
                    }
                    else
                    {
                        ThanhTich = hopDongLD.luongThanhTich ?? 0;
                    }

                }
            }
            return ThanhTich;

        }
        public double Thue(decimal ThanhTich)
        {
            double TienThue = linqNS.fn_thue(Convert.ToDouble(ThanhTich)) ?? 0;
            return TienThue;
        }
        public ActionResult updateListTiLeTT(string[] maNV, int STTTemp)
        {

            //Danh sách chi tiêt nhân viên
            //NhanVienModel modelNV = new NhanVienModel();
            var modelNV = (from p in linqNS.vw_NS_DanhSachNhanVienTheoPhongBans
                           where maNV.Contains(p.maNhanVien)
                           select new NhanVienModel
                           {
                               maNhanVien = p.maNhanVien,
                               hoVaTen = HoVaTen(p.maNhanVien),
                               tenPhongBan = p.tenPhongBan,
                               ThanhTich = TiLeThue(p.maNhanVien),
                               khoanBoSungLuong = 0,
                               Thue = Convert.ToDouble(Thue((TiLeThue(p.maNhanVien)))),
                               STTTemp = STTTemp
                           }).ToList();


            return PartialView("PartialListNV", modelNV);
        }


        /// <summary>
        /// Danh sách nhân viên
        /// </summary>
        /// <param name="page"></param>
        /// <param name="nhomUser"></param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public ActionResult GetMoreUsers(int? page, string maPB, string searchString)
        {
            string nhanVienNhanSu = AdminNhanSu(GetUser().manv);
            if (nhanVienNhanSu == "true")
            {
                maPB = string.Empty;
            }
            int currentPageIndex = page.HasValue ? page.Value : 1;
            int? tongSoDong = 0;
            ViewBag.Search = searchString ?? string.Empty;
            var users = lqPhieuDN.sp_PhieuDeNghi_DanhSachNhanVienTheoPhongBan_Index(maPB, searchString, currentPageIndex, 20).ToList();
            try
            {
                ViewBag.Count = users[0].tongSoDong;
                tongSoDong = users[0].tongSoDong;
            }
            catch
            {
                ViewBag.Count = 0;
            }
            TempData["Params"] = maPB + "," + searchString;
            return PartialView("PartialUsers", users.ToPagedList(currentPageIndex, 20, true, tongSoDong));
        }


        /// <summary>
        /// Thêm và cập nhật thông tin phiếu tăng ca
        /// </summary>
        /// <param name="col"></param>
        /// <param name="isCreate"></param>
        public void BindDataToSave(FormCollection col, bool isCreate)
        {
            if (isCreate == true)
            {
                phieuTL.soPhieu = GenerateUtil.CheckLetter("TLTT", GetMax());
                phieuTL.nguoiLap = GetUser().manv;
                phieuTL.ngayLap = DateTime.Now;
            }
            else
            {
                phieuTL = linqNS.tbl_NS_TiLeThanhTiches.Where(d => d.soPhieu == col.Get("maPhieu")).FirstOrDefault();
                //Delete danh sách nhân viên
                var delNV = linqNS.tbl_NS_TiLeThanhTich_DSNhanViens.Where(d => d.soPhieu == col.Get("maPhieu"));
                linqNS.tbl_NS_TiLeThanhTich_DSNhanViens.DeleteAllOnSubmit(delNV);
            }


            phieuTL.nam = Convert.ToInt32(col.Get("nam"));
            phieuTL.quy = Convert.ToInt32(col.Get("quy"));
            phieuTL.noiDung = col.Get("noiDung");
            phieuTL.xacNhan = col.Get("xacNhan").Contains("true") ? true : false;
            //Insert chi tiết nhân viên 
            string[] maNhanVien = col.GetValues("ValmaNhanVien");
            List<tbl_NS_TiLeThanhTich_DSNhanVien> chiTiet = new List<tbl_NS_TiLeThanhTich_DSNhanVien>();
            tbl_NS_TiLeThanhTich_DSNhanVien ct;
            if (maNhanVien != null && maNhanVien.Count() > 0)
            {
                for (int i = 0; i < maNhanVien.Count(); i++)
                {
                    ct = new tbl_NS_TiLeThanhTich_DSNhanVien();
                    ct.soPhieu = phieuTL.soPhieu;
                    ct.maNhanVien = col.GetValues("ValmaNhanVien")[i];
                    ct.tyle = string.IsNullOrEmpty(col.GetValues("tyLe")[i]) ? 0 : Convert.ToDouble(col.GetValues("tyLe")[i]);
                    ct.luongThanhTich = string.IsNullOrEmpty(col.GetValues("valThanhTich")[i]) ? 0 : Convert.ToDouble(col.GetValues("valThanhTich")[i]);
                    ct.khoanBoSungLuong = string.IsNullOrEmpty(col.GetValues("valKhoanBSLuong")[i]) ? 0 : Convert.ToDouble(col.GetValues("valKhoanBSLuong")[i]);
                    ct.tienThue = string.IsNullOrEmpty(col.GetValues("valThue")[i]) ? 0 : Convert.ToDouble(col.GetValues("valThue")[i]);
                    ct.thucNhan = string.IsNullOrEmpty(col.GetValues("valThucNhan")[i]) ? 0 : Convert.ToDouble(col.GetValues("valThucNhan")[i]);
                    chiTiet.Add(ct);
                }
            }
            if (chiTiet != null && chiTiet.Count > 0)
            {
                linqNS.tbl_NS_TiLeThanhTich_DSNhanViens.InsertAllOnSubmit(chiTiet);
            }
        }
        public ActionResult ImportExcelData(string excelPath)
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
                        string savedLocation = "/UploadFiles/TiLeThanhTich/";
                        Directory.CreateDirectory(savedLocation);
                        var filePath = Server.MapPath(savedLocation);
                        string fileName = date.ToString() + File.FileName;
                        string savedFileName = Path.Combine(filePath, fileName);
                        File.SaveAs(savedFileName);

                        ExcelDataProcessing excelDataProcessor = new ExcelDataProcessing(savedFileName);
                        DataTable dt = excelDataProcessor.GetDataTableWorkSheet("PhieuTiLeThanhTich");
                        var HSMTBanDau = "";
                        var flashBanDau = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            if (flashBanDau == 0)
                            {
                                var maLoaiTNKTemp = row["Tháng"].ToString();
                                HSMTBanDau = maLoaiTNKTemp;
                                flashBanDau = 1;
                                break;
                            }

                        }
                        var thuTuParent = 0;
                        var Flash = 0;
                        var maPhieuTNK = string.Empty;
                        foreach (DataRow row in dt.Rows)
                        {
                            var namCheck = Convert.ToInt32(String.IsNullOrEmpty(row["Năm"].ToString()) ? 0 : Convert.ToInt32(row["Năm"]));
                            var thangCheck = Convert.ToInt32(String.IsNullOrEmpty(row["Tháng"].ToString()) ? 0 : Convert.ToInt32(row["Tháng"]));
                          
                            if (namCheck != 0 && thangCheck != 0)
                            {
                                var maLoaiTNK = row["Tháng"].ToString();
                                if (HSMTBanDau == maLoaiTNK)
                                {
                                    Flash = 0;
                                }
                                else
                                {
                                    HSMTBanDau = maLoaiTNK;
                                    Flash = 1;
                                }
                                if (Flash == 1 || thuTuParent == 0)
                                {

                                    thuTuParent = 1;
                                    Flash = 0;
                                    maPhieuTNK = GenerateUtil.CheckLetter("TLTT", GetMax());
                                    // Check TiLeThanhTichNay da co chua
                                    var nam = Convert.ToInt32(String.IsNullOrEmpty(row["Năm"].ToString()) ? 0 : Convert.ToInt32(row["Năm"]));
                                    var thang = Convert.ToInt32(String.IsNullOrEmpty(row["Tháng"].ToString()) ? 0 : Convert.ToInt32(row["Tháng"]));
                                    var checkTonTai = linqNS.tbl_NS_TiLeThanhTiches.Where(d => d.quy == thang && d.nam == nam).FirstOrDefault();
                                    if (checkTonTai == null)
                                    {
                                        tbl_NS_TiLeThanhTich thuNhapKhacImport = new tbl_NS_TiLeThanhTich();
                                        thuNhapKhacImport.maNhanVien = null;
                                        thuNhapKhacImport.soPhieu = maPhieuTNK;
                                        thuNhapKhacImport.nguoiLap = GetUser().manv;
                                        thuNhapKhacImport.ngayLap = DateTime.Now;
                                        thuNhapKhacImport.nam = String.IsNullOrEmpty(row["Năm"].ToString()) ? 0 : Convert.ToInt32(row["Năm"]);
                                        thuNhapKhacImport.quy = String.IsNullOrEmpty(row["Tháng"].ToString()) ? 0 : Convert.ToInt32(row["Tháng"]);
                                        linqNS.tbl_NS_TiLeThanhTiches.InsertOnSubmit(thuNhapKhacImport);
                                        linqNS.SubmitChanges();


                                    }
                                    else
                                    {
                                        maLoaiTNK = checkTonTai.soPhieu;

                                    }

                                }
                                if (Flash == 0)
                                {
                                    var tenNhanVien = row["Mã nhân viên"].ToString().Trim().ToLower();

                                    var maNhanVien = linqNS.tbl_NS_NhanViens.Where(d => d.maNhanVien == tenNhanVien).Select(d => d.maNhanVien).FirstOrDefault();

                                    if (maNhanVien != null && maNhanVien != "")
                                    {
                                        var thang = Convert.ToInt32(row["Tháng"]);
                                        var nam = Convert.ToInt32(row["Năm"]);


                                        //Check if ma nhan vien
                                        var checkMaNV = (from p in linqNS.tbl_NS_TiLeThanhTich_DSNhanViens
                                                         join q in linqNS.tbl_NS_TiLeThanhTiches on p.soPhieu equals q.soPhieu
                                                         where (p.maNhanVien == maNhanVien && q.quy == thang && q.nam == nam)
                                                         select new
                                                         {
                                                             maNhanVien = p.maNhanVien
                                                         });
                                        if (checkMaNV.Count() == 0)
                                        {


                                            // Import chi tiet
                                            tbl_NS_TiLeThanhTich_DSNhanVien thuNhapKhacImportNhanVien = new tbl_NS_TiLeThanhTich_DSNhanVien();
                                            thuNhapKhacImportNhanVien.maNhanVien = maNhanVien;
                                            thuNhapKhacImportNhanVien.soPhieu = maPhieuTNK;
                                            thuNhapKhacImportNhanVien.tyle = String.IsNullOrEmpty(row["Tỉ lệ"].ToString()) ? 0 : Convert.ToDouble(row["Tỉ lệ"]) * 100;
                                            thuNhapKhacImportNhanVien.khoanBoSungLuong = String.IsNullOrEmpty(row["Khoản bổ sung lương"].ToString()) ? 0 : Convert.ToDouble(row["Khoản bổ sung lương"]);
                                            //decimal LuongTTBanDau = 0;
                                            // Check nhan vien in bang tileThanhTich_LTT.

                                            //var checkTT = linqNS.tbl_NS_TiLeThanhTich_LuongThanhTiches.Where(d => d.tenNhanVien.ToLower().Equals(tenNhanVien.ToLower()) && d.nam == nam && d.thang == thang).ToList().Where(d => (d.tenNhanVien.Trim()).ToLower() == tenNhanVien.ToLower()).FirstOrDefault();
                                            //if (checkTT != null)
                                            //{
                                            //    LuongTTBanDau = checkTT.luongThanhTich ?? 0;
                                            //}
                                            //else
                                            //{
                                            //    LuongTTBanDau = TiLeThue(maNhanVien);
                                            //}
                                            //

                                            //var luongTTTamp = (Convert.ToDecimal(thuNhapKhacImportNhanVien.tyle) * LuongTTBanDau) / 100;
                                            //thuNhapKhacImportNhanVien.luongThanhTich = Convert.ToDouble(luongTTTamp);
                                            //var TienThue = Thue(luongTTTamp + Convert.ToDecimal(thuNhapKhacImportNhanVien.khoanBoSungLuong));
                                            //thuNhapKhacImportNhanVien.tienThue = TienThue;
                                            //thuNhapKhacImportNhanVien.thucNhan = Convert.ToDouble(luongTTTamp) + Convert.ToDouble(thuNhapKhacImportNhanVien.khoanBoSungLuong) - TienThue;

                                            //New
                                            thuNhapKhacImportNhanVien.luongThanhTich = String.IsNullOrEmpty(row["Lương thành tích"].ToString()) ? 0 : Convert.ToDouble(row["Lương thành tích"]);
                                            thuNhapKhacImportNhanVien.tienThue = String.IsNullOrEmpty(row["Thuế TNCN"].ToString()) ? 0 : Convert.ToDouble(row["Thuế TNCN"]);
                                            thuNhapKhacImportNhanVien.thucNhan = String.IsNullOrEmpty(row["Thực nhận"].ToString()) ? 0 : Convert.ToDouble(row["Thực nhận"]);
                                            // New

                                            linqNS.tbl_NS_TiLeThanhTich_DSNhanViens.InsertOnSubmit(thuNhapKhacImportNhanVien);
                                            linqNS.SubmitChanges();

                                        }
                                        else
                                        {
                                            SaveActiveHistory("Đã tồn tại: " + tenNhanVien);
                                        }
                                    }
                                    else
                                    {
                                        SaveActiveHistory("Không tim thấy nhân viên: " + tenNhanVien);
                                    }
                                }

                            }
                        }

                        System.IO.File.Delete(Server.MapPath("/UploadFiles/TiLeThanhTich/" + fileName));
                    }
                }
                SaveActiveHistory("Import danh sách tỉ lệ thành tích.");
                return Json(new { success = true });
            }
            catch
            {
                return View();
            }
        }
        public FileResult DownloadImportFile()
        {
            string savedFileName = Path.Combine("/UploadFiles/Template/", "PhieuTiLeThanhTich.xlsx");
            return File(savedFileName, "multipart/form-data", "PhieuTiLeThanhTich.xlsx");
        }

        // Xuat File Ti le thanh tich
        public void XuatFileTiLeTT(int quy, int nam)
        {

            try
            {

                var filename = "";
                var virtualPath = HttpRuntime.AppDomainAppVirtualPath;

                var fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(virtualPath + @"\Content\Report\ReportTemplate.xls"), FileMode.Open, FileAccess.Read);

                var workbook = new HSSFWorkbook(fileStream, true);
                filename += "TiLeThanhTichNV_Quy" + quy + "_Nam" + nam + ".xls";


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

                string rowtitle = "Tỉ lệ thành tích quý: " + quy + " năm: " + nam;
                var titleCell = HSSFCellUtil.CreateCell(sheet.CreateRow(firstRowNumber), 6, rowtitle.ToUpper());
                titleCell.CellStyle = styleTitle;

                //string cellEnd2 = "";
                //var titleCellEnd2 = HSSFCellUtil.CreateCell(sheet.GetRow(1), 43, cellEnd2.ToUpper());
                //titleCellEnd2.CellStyle = styleTitle;

                ++firstRowNumber;

                var list1 = new List<string>();
                list1.Add("STT");
                list1.Add("Mã nhân viên");
                list1.Add("Họ tên");
                list1.Add("CMND");
                list1.Add("Số tài khoản");
                list1.Add("Tên ngân hàng");
                list1.Add("Tên khối tính lương");
                list1.Add("Tỷ lệ tháng " + quy);
                list1.Add("Tỷ lệ tháng " + (quy + 1));
                list1.Add("Tỷ lệ tháng " + (quy + 2));
                list1.Add("Tổng");
                list1.Add("");
                list1.Add("");




                var list2 = new List<string>();
                list2.Add("STT");
                list2.Add("Mã nhân viên");
                list2.Add("Họ tên");
                list2.Add("CMND");
                list2.Add("Số tài khoản");
                list2.Add("Tên ngân hàng");
                list2.Add("Tên khối tính lương");
                
                list2.Add("Tỷ lệ tháng " + quy);
                list2.Add("Tỷ lệ tháng " + (quy + 1));
                list2.Add("Tỷ lệ tháng " + (quy + 2));
                list2.Add("Lương thành tích");
                list2.Add("Thuế");
                list2.Add("Thực nhận");




                var list3 = new List<string>();
                for (var col = 1; col <= 13; col++)
                {
                    list3.Add("" + col + "");
                }

                var idRowStart = 2; // bat dau o dong thu 4
                var headerRow = sheet.CreateRow(idRowStart);
                int rowend = idRowStart;
                ReportHelperExcel.CreateHeaderRow(headerRow, 0, styleheadedColumnTable, list1);
                idRowStart++;
                var headerRow1 = sheet.CreateRow(idRowStart);
                ReportHelperExcel.CreateHeaderRow(headerRow1, 0, styleheadedColumnTable, list2);

                idRowStart++;
                var headerRow2 = sheet.CreateRow(idRowStart);
                ReportHelperExcel.CreateHeaderRow(headerRow2, 0, styleheadedColumnTable, list3);
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow.RowNum, 10, 12));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 9, 9));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 8, 8));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 7, 7));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 6, 6));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 5, 5));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 4, 4));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 3, 3));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 2, 2));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 1, 1));
                sheet.AddMergedRegion(new CellRangeAddress(headerRow.RowNum, headerRow1.RowNum, 0, 0));



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

                var listThanhTich = linqNS.sp_NS_TiLeThanhTich_XuatFile(quy, nam).ToList();
                var listMaNV = listThanhTich.Select(d => d.maNhanVien).Distinct().ToList();

                var stt = 0;
                int dem = 0;
                if (listMaNV.Count > 0)
                {
                    foreach (var maNV in listMaNV)
                    {
                        var thanhTichNV = listThanhTich.Where(d => d.maNhanVien == maNV).FirstOrDefault();
                        var tongLuongTT = listThanhTich.Where(d => d.maNhanVien == maNV).Sum(d => d.tongLuongTT);
                        var tongtienThue = listThanhTich.Where(d => d.maNhanVien == maNV).Sum(d => d.tongtienThue);
                        var tongthucNhan = listThanhTich.Where(d => d.maNhanVien == maNV).Sum(d => d.tongthucNhan);
                        double valTiLe1 = GetTyLe(quy, nam,maNV);
                        double valTiLe2 = GetTyLe((quy+1), nam, maNV);
                        double valTiLe3 = GetTyLe((quy+2), nam, maNV);
                        dem = 0;

                        idRowStart++;
                        rowC = sheet.CreateRow(idRowStart);
                        ReportHelperExcel.SetAlignment(rowC, dem++, (++stt).ToString(), hStyleConCenter);
                        ReportHelperExcel.SetAlignment(rowC, dem++, maNV, hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(thanhTichNV.hoVaTen), hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(thanhTichNV.CMNDSo), hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(thanhTichNV.soTaiKhoan), hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(thanhTichNV.tenNganHang), hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(thanhTichNV.tenKhoiTinhLuong), hStyleConLeft);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(valTiLe1), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(valTiLe2), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, Convert.ToString(valTiLe3), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongLuongTT ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongtienThue ?? 0), hStyleConRight);
                        ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongthucNhan ?? 0), hStyleConRight);
                    }
                }
                // Sum tong
                stt = 0;
                dem = 0;

                var tongLuongTTSum = listThanhTich.Sum(d => d.tongLuongTT);
                var tongtienThueSum = listThanhTich.Sum(d => d.tongtienThue);
                var tongthucNhanSum = listThanhTich.Sum(d => d.tongthucNhan);


                idRowStart++;
                rowC = sheet.CreateRow(idRowStart);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConCenter);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowC, dem++, "", hStyleConLeft);
                ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongLuongTTSum ?? 0), hStyleConRight);
                ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongtienThueSum ?? 0), hStyleConRight);
                ReportHelperExcel.SetAlignment(rowC, dem++, String.Format("{0:#,##0}", tongthucNhanSum ?? 0), hStyleConRight);
                // End sumtong

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
        public double GetTyLe(int quy, int nam, string maNV) {
            double valTiLe = 0;
            var tile = (from p in linqNS.tbl_NS_TiLeThanhTich_DSNhanViens
                         join q in linqNS.tbl_NS_TiLeThanhTiches on p.soPhieu equals q.soPhieu
                         where (q.quy == quy && q.nam == nam && p.maNhanVien == maNV)
                         select new TiLeThanhTich
                         {
                             tyle = p.tyle ?? 0
                         }).FirstOrDefault();
            if (tile != null)
            {
                valTiLe = tile.tyle;
            }
            return valTiLe;

        }
        // End Xuat File
        public void GetThongTinTiLeThanhTich(string id)
        {
            tiLeThanhTich = new TiLeThanhTich();
            var ds = linqNS.tbl_NS_TiLeThanhTiches.Where(d => d.soPhieu == id).FirstOrDefault();
            tiLeThanhTich.maPhieu = ds.soPhieu;
            tiLeThanhTich.ngayLap = ds.ngayLap;
            tiLeThanhTich.NhanVien = new BatDongSan.Models.NhanSu.NhanVienModel
            {
                maNhanVien = ds.nguoiLap,
                hoVaTen = HoVaTen(ds.nguoiLap)
            };

            tiLeThanhTich.maPhongBan = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == ds.nguoiLap).Select(d => d.maPhongBan).FirstOrDefault() ?? string.Empty;
            tiLeThanhTich.tenPhongBan = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == ds.nguoiLap).Select(d => d.tenPhongBan).FirstOrDefault() ?? string.Empty;

            tiLeThanhTich.noiDung = ds.noiDung;
            tiLeThanhTich.quy = ds.quy ?? 0;
            tiLeThanhTich.nam = ds.nam ?? 0;
            tiLeThanhTich.xacNhan = ds.xacNhan ?? false;
            //Danh sách chi tiêt nhân viên tham dự tăng ca
            ViewBag.ChiTietNhanVien = linqNS.tbl_NS_TiLeThanhTich_DSNhanViens.Where(d => d.soPhieu == id).Select(g => new NhanVienModel
                {
                    maNhanVien = g.maNhanVien,
                    hoVaTen = HoVaTen(g.maNhanVien),
                    //tenChucDanh = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == g.maNhanVien).Select(d => d.TenChucDanh).FirstOrDefault() ?? string.Empty,
                    tenPhongBan = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == g.maNhanVien).Select(d => d.tenPhongBan).FirstOrDefault() ?? string.Empty,
                    //email = linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == g.maNhanVien).Select(d => d.email).FirstOrDefault() ?? string.Empty,
                    tyle = Convert.ToDouble(g.tyle),
                    ThanhTich = Convert.ToDecimal(g.luongThanhTich ?? 0),
                    Thue = g.tienThue ?? 0,
                    TienThucNhan = g.thucNhan ?? 0,
                    khoanBoSungLuong = g.khoanBoSungLuong ?? 0
                    //linqNS.vw_NS_DanhSachNhanVienTheoPhongBans.Where(d => d.maNhanVien == g.maNhanVien).Select(d => d.tenPhongBan).FirstOrDefault() ?? string.Empty,
                }).ToList();

            ViewBag.URL = Request.Url.AbsoluteUri.ToString();

        }
        public int CheckQuyNam(int? Quy, int? Nam)
        {
            var checkList = linqNS.tbl_NS_TiLeThanhTiches.Where(d => d.quy == Quy && d.nam == Nam).FirstOrDefault();
            if (checkList != null)
            {
                return 1;
            }
            return 0;
        }



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
        public ActionResult LoadNhanVien(string qSearch, int _page, string parrentId)
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
                PagingLoaderController("/TiLeThanhTich/Index/", total, page, "?qsearch=" + qSearch + "&parrentId=" + parrentId);
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
