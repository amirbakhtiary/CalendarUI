using AutoMapper;
using CalendarUI.Domain.Entities;
using CalendarUI.Models;
using CalendarUI.Service.Command;
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

        public async Task<ActionResult<Appointment>> CreateAppointment(CreateAppointmentModel createAppointmentModel)
        {
            try
            {
                return await _mediator.Send(new CreateAppointmentCommand
                {
                    appointment = _mapper.Map<Appointment>(createAppointmentModel)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<ActionResult<Appointment>> UpdateAppointment(UpdateAppointmentModel updateAppointmentModel)
        {
            try
            {
                var appointment = await _mediator.Send(new GetAppointmentByIdQuery
                {
                    Id = updateAppointmentModel.Id
                });

                if (appointment == null)
                {
                    return BadRequest($"No appointment found with the id {updateAppointmentModel.Id}");
                }

                return await _mediator.Send(new UpdateAppointmentCommand
                {
                    appointment = _mapper.Map(updateAppointmentModel, appointment)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
