FROM ghcr.io/cirruslabs/flutter:stable AS build
WORKDIR /app
COPY . .

ARG API_URL

RUN flutter pub get
RUN flutter build web --release --dart-define=API_URL=${API_URL}

FROM nginx:alpine
COPY --from=build /app/build/web /usr/share/nginx/html
EXPOSE 80