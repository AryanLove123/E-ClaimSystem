# ---------- Stage 1: compile Angular to static HTML/JS/CSS ----------
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build -- --configuration production

# ---------- Stage 2: serve those static files with nginx ----------
FROM nginx:alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /app/dist/eclaim-ui/browser /usr/share/nginx/html
EXPOSE 80
