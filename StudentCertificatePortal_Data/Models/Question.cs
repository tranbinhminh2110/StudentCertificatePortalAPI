using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public string? QuestionName { get; set; }

    public int? ExamId { get; set; }

    public string? QuestionAnswer { get; set; }

    public bool? CorrectAnswer { get; set; }

    public virtual SimulationExam? Exam { get; set; }
}
