# =====================================================
# 🧩 4 STAGES: base → build → publish → final
# =====================================================

# -----------------------------------------------------
# 🏗️ STAGE 1: Base runtime environment
# Dùng để CHẠY app (chỉ có .NET Runtime, không có SDK)
# -----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# -----------------------------------------------------
# 🧱 STAGE 2: Build environment
# Dùng để BUILD mã nguồn (có đầy đủ .NET SDK)
# -----------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

WORKDIR /src/AuthService

# Copy file .csproj trước để cache bước restore
COPY ["AuthService.csproj", "."]

# Restore các dependency (NuGet packages)
RUN dotnet restore "AuthService.csproj"

# Copy toàn bộ source code vào container
COPY . .

# Build project ở chế độ Release, output vào /app/build
RUN dotnet build "AuthService.csproj" -c Release -o /app/build


# -----------------------------------------------------
# 🚀 STAGE 3: Publish app
# Xuất bản bản release cuối cùng (tối ưu để deploy)
# -----------------------------------------------------
FROM build AS publish

# Publish app ra /app/publish, bỏ host executable (Linux không cần .exe)
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false


# -----------------------------------------------------
# 🧩 STAGE 4: Final runtime image
# Dùng image nhẹ (chỉ runtime) để chạy app đã build
# -----------------------------------------------------
FROM base AS final
WORKDIR /app

# Copy kết quả publish từ stage trước
COPY --from=publish /app/publish .

# Khi container khởi động → chạy app .NET
ENTRYPOINT [ "dotnet", "AuthService.dll" ]
