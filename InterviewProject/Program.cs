using InterviewProject.Repositories;
using InterviewProject.Services;

var folderPatch = "C:\\Users\\shuseynli\\Desktop\\example\\VR GROUP DATA";

var boxRepository = new BoxRepository("Server=localhost; Database=VR.GROUP.TEST; Integrated Security=True;");

var fileImportMonitor = new FileImportMonitor(folderPatch, boxRepository);

fileImportMonitor.StartMonitoring();