using BatDongSan.Helper.Common;
using BatDongSan.Models.DanhMuc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BatDongSan.Utils;
using BatDongSan.Utils.Paging;

namespace BatDongSan.Controllers.DanhMuc
{
    public class QuanLyMayChamCongController : ApplicationController
    {
        private LinqDanhMucDataContext context = new LinqDanhMucDataContext();
        private IList<tbl_DM_ThuNhapKhac> _DMThuNhapKhacs;
        private HT_MayChamCong _recordMCC;
        private readonly string MCV = "QuanLyMayChamCong";
        private bool? permission;
        //
        // GET: /QuanLyMayChamCong/
        #region Load danh sách quản lý mấy chấm công
        public ActionResult Index(int? page, int? pageSize, string searchString, int? trangThai)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenXem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion
            try
            {
                int currentPageIndex = page.HasValue ? page.Value : 1;
                pageSize = pageSize ?? 30;
                int? tongSoDong = 0;
                var nhanVienList = context.sp_NS_MayChamCong_Index(searchString, trangThai, currentPageIndex, pageSize).ToList();
                try
                {
                    ViewBag.Count = nhanVienList[0].tongSoDong;
                    tongSoDong = nhanVienList[0].tongSoDong;
                }
                catch
                {
                    ViewBag.Count = 0;
                }
                TempData["Params"] = searchString + "," + trangThai;
                if (Request.IsAjaxRequest())
                {
                    return PartialView("PartialIndex", nhanVienList.ToPagedList(currentPageIndex, 30, true, tongSoDong));
                }
                return View(nhanVienList.ToPagedList(currentPageIndex, 30, true, tongSoDong));
            }
            catch (Exception ex)
            {
                return View("error");
            }
        }

        #endregion

        #region Thêm, xóa, sửa quản lý máy chấm công
        public ActionResult Create()
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion

                _recordMCC = new HT_MayChamCong();
                _recordMCC.trangThai = true;
                return PartialView("Create", _recordMCC);
            }
            catch (Exception ex)
            {
                return View("error");
            }
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenThem);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                _recordMCC = new HT_MayChamCong();
                _recordMCC.maMay = collection["maMay"];
                _recordMCC.ngayLap = DateTime.Now;
                _recordMCC.nguoiLap = GetUser().manv;
                _recordMCC.tenMay = collection["tenMay"];
                _recordMCC.tenCongTrinh = collection["tenCongTrinh"];
                _recordMCC.trangThai = collection["trangThai"].Contains("true");
                _recordMCC.ghiChu = collection["ghiChu"];

                var obj = context.HT_MayChamCongs.Where(s => s.maMay == _recordMCC.maMay).FirstOrDefault();
                if (obj != null)
                {
                    TempData["TonTai"] = "Lưu không thành công - mã mãy chấm công đã tồn tại.";
                    return RedirectToAction("Index");
                }
                context.HT_MayChamCongs.InsertOnSubmit(_recordMCC);
                context.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("error");
            }
        }

        public ActionResult Edit(string id)
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion

                _recordMCC = context.HT_MayChamCongs.Where(s => s.maMay == id).FirstOrDefault();
                return PartialView("Edit", _recordMCC);
            }
            catch (Exception ex)
            {
                return View("error");
            }
        }

        [HttpPost]
        public ActionResult Edit(FormCollection collection)
        {
            #region Role user
            permission = GetPermission(MCV, BangPhanQuyen.QuyenSua);
            if (!permission.HasValue)
                return View("LogIn");
            if (!permission.Value)
                return View("AccessDenied");
            #endregion

            try
            {
                _recordMCC = context.HT_MayChamCongs.Where(s => s.maMay == collection["maMay"]).FirstOrDefault();
                _recordMCC.tenMay = collection["tenMay"];
                _recordMCC.tenCongTrinh = collection["tenCongTrinh"];
                _recordMCC.trangThai = collection["trangThai"].Contains("true");
                _recordMCC.ghiChu = collection["ghiChu"];
                context.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("error");
            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                #region Role user
                permission = GetPermission(MCV, BangPhanQuyen.QuyenXoa);
                if (!permission.HasValue)
                    return View("LogIn");
                if (!permission.Value)
                    return View("AccessDenied");
                #endregion
                _recordMCC = context.HT_MayChamCongs.Where(s => s.maMay == id).FirstOrDefault();
                context.HT_MayChamCongs.DeleteOnSubmit(_recordMCC);
                context.SubmitChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("error");
            }
        }
        #endregion
    }
}
