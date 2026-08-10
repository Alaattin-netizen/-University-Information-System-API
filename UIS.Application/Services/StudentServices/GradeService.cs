using UIS.Application.Abstractions.StudentAbstractions;
using UIS.Application.DTOs.Student.Grades;
using UIS.Domain.Entities;
using UIS.Infrastructure.Repositories;

namespace UIS.Application.Services.StudentServices;

public class GradeService : IGradeService
{
    private readonly IUnitOfWork _unitOfWork;

    public GradeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<GradeResponse>> GetGradesAsync(int studentId)
    {
        var repo = _unitOfWork.Repository<Enrollment>();
        var enrollments = await repo.FindAsync(e =>
            e.StudentId == studentId &&
            e.IsActive &&
            e.LetterGrade != null);

        return enrollments.Select(e => new GradeResponse
        {
            CourseCode = e.CourseOffering.Course.Code,
            CourseName = e.CourseOffering.Course.Name,
            Credits = e.CourseOffering.Course.Credits,
            Midterm = e.MidtermScore,
            Final = e.FinalScore,
            TotalScore = e.TotalScore,
            LetterGrade = e.LetterGrade,
            GradePoint = e.GradePoint ?? 0
        });
    }

    public async Task<GPAResponse> GetGPAAsync(int studentId)
    {
        var repo = _unitOfWork.Repository<Enrollment>();
        var enrollments = await repo.FindAsync(e =>
            e.StudentId == studentId &&
            e.IsActive &&
            e.LetterGrade != null);

        // Get current semester (active semester)
        var semesterRepo = _unitOfWork.Repository<Semester>();
        var currentSemester = await semesterRepo.GetFirstAsync(s => s.IsActive);

        var currentEnrollments = enrollments.Where(e =>
            e.CourseOffering.SemesterId == currentSemester?.Id).ToList();

        double semesterTotalPoints = 0;
        int semesterTotalCredits = 0;

        foreach (var e in currentEnrollments)
        {
            semesterTotalPoints += (e.GradePoint ?? 0) * e.CourseOffering.Course.Credits;
            semesterTotalCredits += e.CourseOffering.Course.Credits;
        }

        double semesterGpa = semesterTotalCredits > 0 ? semesterTotalPoints / semesterTotalCredits : 0;

        // Cumulative GPA
        double cumulativeTotalPoints = 0;
        int cumulativeTotalCredits = 0;

        foreach (var e in enrollments)
        {
            cumulativeTotalPoints += (e.GradePoint ?? 0) * e.CourseOffering.Course.Credits;
            cumulativeTotalCredits += e.CourseOffering.Course.Credits;
        }

        double cumulativeGpa = cumulativeTotalCredits > 0 ? cumulativeTotalPoints / cumulativeTotalCredits : 0;

        return new GPAResponse
        {
            SemesterGPA = Math.Round(semesterGpa, 2),
            CumulativeGPA = Math.Round(cumulativeGpa, 2),
            TotalCredits = cumulativeTotalCredits
        };
    }

    public async Task<IEnumerable<GradeResponse>> GetTranscriptAsync(int studentId)
    {
        // Same as GetGradesAsync, but typically includes ALL semesters
        return await GetGradesAsync(studentId);
    }
}