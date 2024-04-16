// See https://aka.ms/new-console-template for more information
using InterviewProject.Services;

string folderPatch = "D:\\Projects\\Tasks_VR_GROUP\\VR_Challenge_Senior_backend_developer";

var fileImportMonitor = new FileImportMonitor(folderPatch);

fileImportMonitor.StartMonitoring();
