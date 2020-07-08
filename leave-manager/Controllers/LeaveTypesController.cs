using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_manager.Contracts;
using leave_manager.Data;
using leave_manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace leave_manager.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class LeaveTypesController : Controller
    {
        private readonly ILeaveTypeRepository _repo;
        private readonly IMapper _mapper;

        public LeaveTypesController(ILeaveTypeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        //GET: LeaveTypes
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _repo.FindAll();
            var model = _mapper.Map<List<LeaveType>, List<LeaveTypeViewModel>>(leaveTypes.ToList());
            return View(model);
        }

        //GET: LeaveTypes/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var isExists = await _repo.isExists(id);
            if (!isExists)
            {
                return NotFound();
            }
            var leaveType = await _repo.FindById(id);
            var model = _mapper.Map<LeaveTypeViewModel>(leaveType);

            return View(model);
        }

        //GET: LeaveTypes/Create
        public ActionResult Create()
        {
            
            return View();
        }

        //POST: LeaveTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(LeaveTypeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var leaveType = _mapper.Map<LeaveType>(model);
                leaveType.DateCreated = DateTime.Now;
                var isSuccess = await _repo.Create(leaveType);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong! Please try again.");
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong! Please try again.");
                return View(model);
            }
        }

        //GET: LeaveTypes/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var isExists = await _repo.isExists(id);
            if (!isExists)
            {
                return NotFound();
            }
            var leaveType = await _repo.FindById(id);
            var model = _mapper.Map<LeaveTypeViewModel>(leaveType);
            return View(model);
        }

        //POST: LeaveTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(LeaveTypeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var leaveType = _mapper.Map<LeaveType>(model);
                var isSuccess = await _repo.Update(leaveType);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong! Please try again.");
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong! Please try again.");
                return View(model);
            }
        }

        //GET: LeaveTypes/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var leaveType = await _repo.FindById(id);
            if (leaveType == null)
            {
                return NotFound();
            }
            var isSuccess = await _repo.Delete(leaveType);
            if (!isSuccess)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(Index));
        }

        //POST: LeaveTypes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, LeaveTypeViewModel model)
        {
            try
            {
                var leaveType = await _repo.FindById(id);
                if (leaveType == null)
                {
                    return NotFound();
                }
                var isSuccess = await _repo.Delete(leaveType);
                if (!isSuccess)
                {
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(model);
            }
        }
    }
}
