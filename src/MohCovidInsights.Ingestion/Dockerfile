FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY global.json Directory.Build.props ./
COPY src/MohCovidInsights.Domain/*.csproj src/MohCovidInsights.Domain/
COPY src/MohCovidInsights.Application/*.csproj src/MohCovidInsights.Application/
COPY src/MohCovidInsights.Infrastructure/*.csproj src/MohCovidInsights.Infrastructure/
COPY src/MohCovidInsights.Ingestion/*.csproj src/MohCovidInsights.Ingestion/
RUN dotnet restore src/MohCovidInsights.Ingestion/MohCovidInsights.Ingestion.csproj

COPY src/MohCovidInsights.Domain/ src/MohCovidInsights.Domain/
COPY src/MohCovidInsights.Application/ src/MohCovidInsights.Application/
COPY src/MohCovidInsights.Infrastructure/ src/MohCovidInsights.Infrastructure/
COPY src/MohCovidInsights.Ingestion/ src/MohCovidInsights.Ingestion/
RUN DOTNET_gcServer=0 dotnet publish src/MohCovidInsights.Ingestion/MohCovidInsights.Ingestion.csproj \
    -c Release -o /app/publish \
    --no-restore \
    -p:PublishReadyToRun=false \
    -p:DebugType=none \
    -p:GenerateDocumentationFile=false

FROM public.ecr.aws/lambda/dotnet:9
WORKDIR /var/task
COPY --from=build /app/publish .
CMD ["MohCovidInsights.Ingestion"]