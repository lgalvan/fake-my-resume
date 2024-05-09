﻿using FakeMyResume.Models;
using FakeMyResume.Models.Data;
using FakeMyResume.Services.Data;

namespace FakeMyResume.Services;

public class ResumeService
{
    private readonly FakeMyResumeDbContext _context;

    public ResumeService(FakeMyResumeDbContext context)
    {
        _context = context;
    }

    public DataResume GetResume(int id)
    {
        return _context.DataResume.First(x => x.Id == id);
    }

    public IEnumerable<DataResume> GetResumes(string userId)
    {
        return _context.DataResume.Where(resume => resume.UserId.Equals(userId));
    }

    public DataResume SaveResume(Resume resume, string accountId)
    {
        //try
        //{
            var dataResume = new DataResume

            {
                UserId = accountId,
                ResumeData = resume,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
            _context.DataResume.Add(dataResume);
            _context.SaveChanges();
            return GetResume(dataResume.Id);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.ToString());
        //}
        //return new DataResume { UserId = accountId, ResumeData = resume };
    }

    public DataResume UpdateResume(int id, Resume resume)
    {
        var dataResume = GetResume(id);
        dataResume.ResumeData = resume;
        dataResume.LastUpdated = DateTime.UtcNow;
        _context.DataResume.Update(dataResume);
        _context.SaveChanges();
        return dataResume;
    }
}
