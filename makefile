publish: publish_vue publish_dotnet move_vue_to_dotnet generate_installer

publish_vue:
	cd .\Client && npm run build

publish_dotnet:
	cd .\Server && dotnet publish

move_vue_to_dotnet:
	if exist .\Server\bin\Release\net8.0-windows\wwwroot rd /s /q ".\Server\bin\Release\net8.0-windows\wwwroot"
	xcopy ".\Client\dist\" ".\Server\bin\Release\net8.0-windows\wwwroot\" /e /s /y

generate_installer:
	iscc "innoSetup.iss"