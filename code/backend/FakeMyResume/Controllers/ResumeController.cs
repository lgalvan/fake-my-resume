using AutoMapper;
using FakeMyResume.DTOs;
using FakeMyResume.Models;
using FakeMyResume.Models.Data;
using FakeMyResume.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI.ObjectModels.ResponseModels;
using System.Security.Claims;

namespace FakeMyResume.Controllers;

[ApiController, Authorize, Route("api/resume")]
//[ApiController, Route("api/resume")]
public class ResumeController(ResumeService resumeService, DocumentGenerationService documentGenerationService,UserService userService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAccountResumes()
    {
        var accountId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (accountId == null)
            return new UnauthorizedResult();

        var resumes = resumeService.GetResumes(accountId).Select(ResumeDTO.FromData);
        return Ok(resumes);
    }

    [HttpGet("{id}")]
    public IActionResult GetResume(int id)
    {
        var accountId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var resume = resumeService.GetResume(id);

        if (resume == null)
        {
            return NotFound();
        }

        if (!resume.UserId.Equals(accountId))
        {
            return Unauthorized($"The requested resume belongs to another user.");
        }

        var result = ResumeDTO.FromData(resume);
        return Ok(result);
    }

    [HttpGet("{id}/pdf")]
    public IActionResult GetResumePDF(int id)
    {
        try
        {
            var resume = resumeService.GetResume(id);            
            return File(documentGenerationService.GenerateResumeInPDF(resume.ResumeData), "application/pdf");
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet("techs")]
    public IActionResult GetTechnologies([FromQuery] String text)
    {
        List<string> list = new List<string> { "C#", "JAVA", "JS", "VUE", "ANGULAR", "AWS","LUA","GROOVY","JENKINS","PHP","NODE","NEXT","RUST","RUBY","GO","R" };

        var incoming = text.ToUpper();
        var possibleMatches= list.Where(x=>x.StartsWith(incoming));

        return Ok(possibleMatches);
    }

    [HttpPost]
    public  IActionResult SaveResume(CreateResumeDTO resumeDTO)//FIX this
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        /*
         // TODO we need to figure out which one we need Claim or user??
        //depending on the answer for this we need to change the parameter of resumeService.SaveResume() used below
        var accountId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        */
        var accountData = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;//object identifer

        User accountId =  userService.GetUserByUserName("tdata");// new Claim("type", "value");//temp force it while we figure out what is its purpose
        if (accountId == null)
            return new UnauthorizedResult();

      
        Resume resume = GetResumeFromDto(resumeDTO);//TODO use automapper? or what was the idea here? regardless of the answer we need to move code outside of the controller

        var newResume = resumeService.SaveResume(resume, accountId.Id);
        // var result = ResumeDTO.FromData(newResume);
        //return Created($"/api/resume/{newResume.Id}",newResume);
        return Created(string.Empty,null);
        //return Ok();//TODO which one is the correct return??  OK or created?? if created the url should change otherwise we get an error
     }

    WorkExperience GetWorkExperienceFromDto(WorkExperienceDTO workExperienceDTO) {
        WorkExperience workExperience = new();
        workExperience.DateBegin = workExperienceDTO.DateBegin;
        workExperience.DateEnd=  workExperienceDTO.DateEnd==null ?  DateTime.Now: (DateTime)workExperienceDTO.DateEnd;
        workExperience.CompanyName= workExperienceDTO.CompanyName == null ? "": (string)workExperienceDTO.CompanyName;
        workExperience.ProjectName= workExperienceDTO.ProjectName == null ? "" : (string)workExperienceDTO.ProjectName; 
        workExperience.Role= workExperienceDTO.Role == null ? "" : (string)workExperienceDTO.Role; 
        workExperience.Description= workExperienceDTO.Description;
        workExperience.Technologies = workExperienceDTO.Technologies.ToList<string>();
        return workExperience;
    }

    Education GetEducaionFromDto(EducationDTO educationDTO) {
        Education education=new();
        education.Degree = educationDTO.Degree;
        education.Major = educationDTO.Major;
        education.UniversityName=educationDTO.UniversityName;
        education.YearOfCompletion= educationDTO.YearOfCompletion;
        education.Country= educationDTO.Country;
        education.State= educationDTO.State;
        return education;
    }

    WorkExperience[] GetWorkExperiences(WorkExperienceDTO[] workExperiences)
    {
        List<WorkExperience> workExperience = new();
        foreach (var wExp in workExperiences)
        { workExperience.Add(GetWorkExperienceFromDto(wExp)); }
        return workExperience.ToArray();
    }

    Education[] GetEducationArray(EducationDTO[] education)
    {
        List<Education> workExperience = new();
        foreach (var wExp in education)
        { workExperience.Add(GetEducaionFromDto(wExp)); }
        return workExperience.ToArray();
    }
    Resume GetResumeFromDto(CreateResumeDTO resumeDTO)
    {
        Resume resume = new();
        resume.FullName = resumeDTO.FullName;
        resume.CurrentRole = resumeDTO.CurrentRole;
        resume.Description = resumeDTO.Description;
        resume.Certifications = resumeDTO.Certifications.ToArray();

        resume.WorkExperience = GetWorkExperiences(resumeDTO.WorkExperience).ToList<WorkExperience>();
        resume.Education = GetEducationArray( resumeDTO.Education).ToList<Education>();

        return resume;
        /*
       var config = new MapperConfiguration(cfg => cfg.CreateMap<Resume, CreateResumeDTO>());

      // var mapper = config.CreateMapper();
       // or
       var mapper = new Mapper(config);
       Resume dto = mapper.Map<CreateResumeDTO>(source: resumeDTO);
      */

    }

    [HttpPut("{id}")]
    public IActionResult UpdateResume(int id, [FromBody] UpdateResumeDTO resumeDTO)
    {        

        var updatedResume = resumeService.UpdateResume(id, new Resume());
        if (updatedResume == null)
        {
            return NotFound();
        }

        var result = ResumeDTO.FromData(updatedResume);
        return Ok(result);
    }

    [HttpGet("dummy")]
    public IActionResult GetDummy()
    {
        return File(documentGenerationService.GenerateResumeInPDF(new Resume()), "application/pdf");
    }
}
