**Exporting Project Templates**
Use Project->Export Template...
1. Edit wizard fields as specified below for each project
2. Copy .zip file to this solution folder
3. Edit the LazyStackVsExt/source.extensions.vsixmanifest to include any new templates
4. Editing the vsixmanifest can be confusing. Make sure you use a relative path to the 
   ExportedTemplates folder when you add the template refernece. See the other existing
   entries for examples.

LazyStackWebApi Template Description:
LazyStackWebApi template is used to create new LazyStack applciation solution

LazyStackAuth Template Description:
LazyStackAuth template provides authentication/authorization for applications generated with  LazyStack 


