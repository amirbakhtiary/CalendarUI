using AutoMapper;
using CalendarUI.Models;
using CalendarUI.Service.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarUI.Controllers
{
    public class CalendarController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public CalendarController(IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        [Route("calendar")]
        public IActionResult Index()
        {
            var monthModel = new List<MonthViewModel>();
            monthModel = Enumerable.Range(1, 12)
                .Select(i => new MonthViewModel
                {
                    Id = i,
                    Name = DateTimeFormatInfo.CurrentInfo.GetMonthName(i).Substring(0, 3)
                })
                .ToList();

            return View(monthModel);
        }

        [Route("appointments/{month}")]
        public async Task<IActionResult> GetAppointments(int month)
        {
            var appointments = await _mediator.Send(
                new GetAppointmentsByMonthQuery
                {
                    Month = month
                });
            return PartialView("_Appointments", appointments);
        }

        [Route("appointmentdetail/{id}")]
        public async Task<IActionResult> GetAppointmentDetail(Guid id)
        {
            var appointment = await _mediator.Send(
                new GetAppointmentByIdQuery
                {
                    Id = id
                });
            return PartialView("_AppointmentDetail", appointment);
        }
    }
}
