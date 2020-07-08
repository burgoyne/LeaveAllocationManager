using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_manager.Contracts;
using leave_manager.Data;
using leave_manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace leave_manager.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class LeaveAllocationController : Controller
    {
        private readonly ILeaveTypeRepository _typeRepo;
        private readonly ILeaveAllocationRepository _allocationRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveAllocationController(ILeaveTypeRepository typeRepo, ILeaveAllocationRepository allocationRepo, IMapper mapper, UserManager<Employee> userManager)
        {
            _allocationRepo = allocationRepo;
            _typeRepo = typeRepo;
            _mapper = mapper;
            _userManager = userManager;
        }
        // GET: LeaveAllocationController
        public async Task<ActionResult> Index()
        {
            var leaveTypes = await _typeRepo.FindAll();
            var mappedLeaveTypes = _mapper.Map<List<LeaveType>, List<LeaveTypeViewModel>>(leaveTypes.ToList());
            var model = new CreateLeaveAllocationViewModel
            {
                LeaveTypes = mappedLeaveTypes,
                NumberUpdated = 0
            };
            return View(model);
        }

        public async Task<ActionResult> SetLeave(int id)
        {
            var leaveType = await _typeRepo.FindById(id);
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            foreach (var emp in employees)
            {
                if (await _allocationRepo.CheckAllocation(id, emp.Id))
                {
                    continue;
                }
                var allocation = new LeaveAllocationViewModel
                {
                    DateCreated = DateTime.Now,
                    EmployeeId = emp.Id,
                    LeaveTypeId = id,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = DateTime.Now.Year
                };
                var leaveAllocation = _mapper.Map<LeaveAllocation>(allocation);
                await _allocationRepo.Create(leaveAllocation);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ListEmployees()
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var model = _mapper.Map<List<EmployeeViewModel>>(employees);
            return View(model);
        }

        // GET: LeaveAllocationController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            var employee = _mapper.Map<EmployeeViewModel>(await _userManager.FindByIdAsync(id));
            var allocations = _mapper.Map<List<LeaveAllocationViewModel>>(await _allocationRepo.GetAllocationsByEmployee(id));
            var model = new ViewAllocationsViewModel
            {
                Employee = employee,
                LeaveAllocations = allocations
            };
            return View(model);
        }

        // GET: LeaveAllocationController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LeaveAllocationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveAllocationController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var model = _mapper.Map<EditLeaveAllocationViewModel>(await _allocationRepo.FindById(id));
            return View(model);
        }

        // POST: LeaveAllocationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditLeaveAllocationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var record = await _allocationRepo.FindById(model.Id);
                record.NumberOfDays = model.NumberOfDays;
                var isSuccess = await _allocationRepo.Update(record);
                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Something went wrong while saving your changes! Please try again.");
                    return View(model);
                }

                return RedirectToAction(nameof(Details), new { id = model.EmployeeId });
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong while saving your changes! Please try again.");
                return View(model);
            }
        }

        // GET: LeaveAllocationController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveAllocationController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
