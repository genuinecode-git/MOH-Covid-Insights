FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY src/MohCovidInsights.Domain/ src/MohCovidInsights.Domain/
COPY src/MohCovidInsights.Application/ src/MohCovidInsights.Application/
COPY src/MohCovidInsights.Infrastructure/ src/MohCovidInsights.Infrastructure/
COPY src/MohCovidInsights.Ingestion/ src/MohCovidInsights.Ingestion/
RUN dotnet publish src/MohCovidInsights.Ingestion/MohCovidInsights.Ingestion.csproj \
    -c Release -o /app/publish \
    --no-restore \
    -p:DebugType=none \
    -p:GenerateDocumentationFile=false
    
FROM public.ecr.aws/lambda/dotnet:9 AS runtime
WORKDIR /var/task
COPY --from=build /app/publish .
CMD ["MohCovidInsights.Ingestion"]