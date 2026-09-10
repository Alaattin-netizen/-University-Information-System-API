using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using UIS.Application.Abstractions.AdminAbstractions;
using UIS.Application.DTOs.Admin;
using UIS.Application.DTOs.Admin.Attendance;
using UIS.Application.DTOs.Filters;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.AdminServices;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<AttendanceResponse> CreateAsync(CreateAttendanceRequest request)
    {
        var student = await _unitOfWork.Repository<User>().GetByIdAsync(request.StudentId);
        if (student == null) throw new InvalidOperationException("Student not found.");
        var offering = await _unitOfWork.Repository<CourseOffering>().GetByIdAsync(request.CourseOfferingId);
        if (offering == null) throw new InvalidOperationException("Course offering not found.");

        var attendance = new Attendance
        {
            StudentId = request.StudentId,
            CourseOfferingId = request.CourseOfferingId,
            Date = request.Date,
            IsPresent = request.IsPresent
        };

        await _unitOfWork.Repository<Attendance>().AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync();
        return await GetByIdAsync(attendance.Id);
    }

    public async Task<AttendanceResponse> UpdateAsync(UpdateAttendanceRequest request)
    {
        var attendance = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .FirstOrDefaultAsync(a => a.Id == request.Id);

        if (attendance == null) throw new InvalidOperationException("Attendance record not found.");

        attendance.IsPresent = request.IsPresent;
        _unitOfWork.Repository<Attendance>().Update(attendance);
        await _unitOfWork.SaveChangesAsync();
        return MapToResponse(attendance);
    }

    public async Task DeleteAsync(int id)
    {
        var attendance = await _unitOfWork.Repository<Attendance>().GetByIdAsync(id);
        if (attendance == null) throw new InvalidOperationException("Attendance record not found.");
        _unitOfWork.Repository<Attendance>().Delete(attendance);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AttendanceResponse> GetByIdAsync(int id)
    {
        var a = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (a == null) throw new InvalidOperationException("Attendance record not found.");
        return MapToResponse(a);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetAllAsync(AttendanceFilterRequest? filter, int? instructorId = null)
    {
        var list = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .Where(a => !instructorId.HasValue || a.CourseOffering.InstructorId == instructorId.Value)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
        if (filter != null)
        {

            if (filter.IsPresent.HasValue)
            {
                list = list.Where(a => a.IsPresent == filter.IsPresent.Value).ToList();
            }
            if (filter.CourseOfferingId.HasValue)
            {
                list = list.Where(a => a.CourseOfferingId == filter.CourseOfferingId.Value).ToList();
            }
            if (filter.StudentId.HasValue)
            {
                list = list.Where(a => a.StudentId == filter.StudentId.Value).ToList();
            }
            if (filter.FromDate.HasValue)
            {
                list = list.Where(a => a.Date >= filter.FromDate.Value).ToList();
            }
            if (filter.ToDate.HasValue)
            {
                list = list.Where(a => a.Date <= filter.ToDate.Value).ToList();
            }
        }
        return list.Select(MapToResponse);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetByStudentAsync(int studentId)
    {
        var list = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetByCourseOfferingAsync(int courseOfferingId)
    {
        var list = await _unitOfWork.Repository<Attendance>()
            .GetQueryable()
            .Include(a => a.Student)
            .Include(a => a.CourseOffering).ThenInclude(o => o.Course)
            .Where(a => a.CourseOfferingId == courseOfferingId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return list.Select(MapToResponse);
    }

    public async Task<byte[]> ExportAsync(AttendanceFilterRequest? filter = null, int? instructorId = null)
    {
        var rows = (await GetAllAsync(filter, instructorId)).ToList();
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance");
        worksheet.Cell(1, 1).Value = "Date";
        worksheet.Cell(1, 2).Value = "Class";
        worksheet.Cell(1, 3).Value = "Student";
        worksheet.Cell(1, 4).Value = "Student Name";
        worksheet.Cell(1, 5).Value = "Attendance Status";

        for (var i = 0; i < rows.Count; i++)
        {
            var row = i + 2;
            worksheet.Cell(row, 1).Value = rows[i].Date.Date;
            worksheet.Cell(row, 1).Style.DateFormat.Format = "yyyy-MM-dd";
            worksheet.Cell(row, 2).Value = rows[i].CourseCode ?? string.Empty;
            worksheet.Cell(row, 3).Value = rows[i].StudentEmail ?? rows[i].StudentName;
            worksheet.Cell(row, 4).Value = rows[i].StudentName;
            worksheet.Cell(row, 5).Value = rows[i].IsPresent ? "Present" : "Absent";
        }

        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<AttendanceImportResult> ImportAsync(Stream file, int? instructorId = null)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("The attendance Excel file is empty.");

        using var workbook = new XLWorkbook(file);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
            throw new InvalidOperationException("The Excel file does not contain a worksheet.");

        var headers = worksheet.Row(1).CellsUsed()
            .ToDictionary(c => Normalize(c.GetString()), c => c.Address.ColumnNumber);
        var required = new[] { "date", "class", "student", "attendancestatus" };
        var missing = required.Where(h => !headers.ContainsKey(h)).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException($"Missing required columns: {string.Join(", ", missing)}.");

        var result = new AttendanceImportResult();
        var attendanceRepository = _unitOfWork.Repository<Attendance>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);
            if (row.IsEmpty())
                continue;

            var dateCell = row.Cell(headers["date"]);
            var className = row.Cell(headers["class"]).GetString().Trim();
            var studentValue = row.Cell(headers["student"]).GetString().Trim();
            var statusValue = row.Cell(headers["attendancestatus"]).GetString().Trim();
            if (!TryReadDate(dateCell, out var date))
            {
                result.Errors.Add($"Row {rowNumber}: Date must be a valid date.");
                continue;
            }
            date = date.Date;
            if (string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(studentValue))
            {
                result.Errors.Add($"Row {rowNumber}: Class and Student are required.");
                continue;
            }
            if (!TryReadStatus(statusValue, out var isPresent))
            {
                result.Errors.Add($"Row {rowNumber}: Attendance Status must be Present or Absent.");
                continue;
            }

            var existingAttendance = await attendanceRepository.GetQueryable()
                .Include(a => a.CourseOffering)
                    .ThenInclude(o => o.Course)
                .Where(a => a.Date.Date == date
                    && a.CourseOffering.Course.Code == className
                    && (!instructorId.HasValue || a.CourseOffering.InstructorId == instructorId.Value))
                .ToListAsync();

            var offerings = await _unitOfWork.Repository<CourseOffering>().GetQueryable()
                .Include(o => o.Course)
                .Include(o => o.Semester)
                .Where(o => o.Course.Code == className
                    && (!instructorId.HasValue || o.InstructorId == instructorId.Value))
                .ToListAsync();
            var existingOfferingIds = existingAttendance
                .Select(a => a.CourseOfferingId)
                .Distinct()
                .ToList();
            if (existingOfferingIds.Count == 1)
            {
                offerings = new List<CourseOffering>
                {
                    existingAttendance.First(a => a.CourseOfferingId == existingOfferingIds[0]).CourseOffering
                };
            }
            else
            {
                offerings = offerings.Where(o => date >= o.Semester.StartDate.Date
                    && date <= o.Semester.EndDate.Date).ToList();
            }
            if (offerings.Count == 0)
            {
                result.Errors.Add($"Row {rowNumber}: No course offering for class '{className}' exists on {date:yyyy-MM-dd}.");
                continue;
            }
            if (offerings.Count > 1)
            {
                result.Errors.Add($"Row {rowNumber}: More than one course offering for class '{className}' exists on {date:yyyy-MM-dd}.");
                continue;
            }

            var offering = offerings[0];
            var student = await FindStudentAsync(studentValue);
            if (student == null)
            {
                result.Errors.Add($"Row {rowNumber}: Student '{studentValue}' was not found.");
                continue;
            }
            var enrolled = await _unitOfWork.Repository<Enrollment>().GetFirstAsync(e =>
                e.StudentId == student.Id && e.CourseOfferingId == offering.Id && e.IsActive);
            if (enrolled == null)
            {
                result.Errors.Add($"Row {rowNumber}: Student '{studentValue}' is not enrolled in class '{className}'.");
                continue;
            }

            var existing = existingAttendance.FirstOrDefault(a =>
                a.StudentId == student.Id && a.CourseOfferingId == offering.Id);
            if (existing == null)
            {
                await attendanceRepository.AddAsync(new Attendance
                {
                    StudentId = student.Id,
                    CourseOfferingId = offering.Id,
                    Date = date,
                    IsPresent = isPresent
                });
                result.Created++;
            }
            else
            {
                existing.IsPresent = isPresent;
                existing.Date = date;
                attendanceRepository.Update(existing);
                result.Updated++;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return result;
    }

    private async Task<User?> FindStudentAsync(string value)
    {
        if (int.TryParse(value, out var id))
            return await _unitOfWork.Repository<User>().GetByIdAsync(id);

        var normalized = value.Trim().ToLower();
        var students = await _unitOfWork.Repository<User>().GetQueryable().ToListAsync();
        return students.FirstOrDefault(s => s.Email.ToLower() == normalized)
            ?? students.FirstOrDefault(s => $"{s.FirstName} {s.LastName}".ToLower() == normalized);
    }

    private static string Normalize(string value) =>
        new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static bool TryReadDate(IXLCell cell, out DateTime date)
    {
        if (cell.TryGetValue<DateTime>(out date))
            return true;
        return DateTime.TryParse(cell.GetString(), out date);
    }

    private static bool TryReadStatus(string value, out bool isPresent)
    {
        if (value.Equals("present", StringComparison.OrdinalIgnoreCase) || value == "1" ||
            value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            isPresent = true;
            return true;
        }
        if (value.Equals("absent", StringComparison.OrdinalIgnoreCase) || value == "0" ||
            value.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            isPresent = false;
            return true;
        }
        isPresent = false;
        return false;
    }

    private AttendanceResponse MapToResponse(Attendance a) => new()
    {
        Id = a.Id,
        StudentId = a.StudentId,
        StudentName = $"{a.Student.FirstName} {a.Student.LastName}",
        StudentEmail = a.Student.Email,
        CourseOfferingId = a.CourseOfferingId,
        CourseCode = a.CourseOffering?.Course?.Code,
        Date = a.Date,
        IsPresent = a.IsPresent
    };
}