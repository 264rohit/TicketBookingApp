# Build stage for .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /src
COPY ["TicketBookingApp/TicketBookingApp.csproj", "TicketBookingApp/"]
RUN dotnet restore "TicketBookingApp/TicketBookingApp.csproj"
COPY . .
RUN dotnet publish "TicketBookingApp/TicketBookingApp.csproj" -c Release -o /app/publish

# Build stage for React frontend
FROM node:18 AS frontend-build
WORKDIR /client
COPY ["TicketBookingApp/client/package.json", "TicketBookingApp/client/package-lock.json", "./"]
RUN npm ci --legacy-peer-deps
COPY ["TicketBookingApp/client/", "."]
RUN npm run build

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=dotnet-build /app/publish .

# Copy built React frontend into wwwroot
COPY --from=frontend-build /client/dist ./wwwroot

# Expose port (Render uses PORT env var)
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "TicketBookingApp.dll"]
