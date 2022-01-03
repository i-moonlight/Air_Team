### STAGE 1: Build ###
FROM node:14-alpine AS build
ENV CI=true
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run release

### STAGE 2: Run ###
FROM nginx:alpine
COPY --from=build /app/dist/AirTeamUI /usr/share/nginx/html

WORKDIR /etc/nginx/conf.d
RUN echo 'server { location / { try_files $uri /index.html; } }' > spa.conf
WORKDIR /

