FROM node:22-alpine AS build
WORKDIR /app
COPY client/package.json client/package-lock.json ./
RUN npm ci
COPY client/ .
RUN npm run build

FROM nginx:alpine
COPY client/nginx.conf /etc/nginx/templates/default.conf.template
COPY --chmod=755 client/reload-nginx.sh /docker-entrypoint.d/99-reload-nginx.sh
COPY --from=build /app/dist /usr/share/nginx/html
