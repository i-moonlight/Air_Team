### STAGE 1: Build ###
FROM node:16-alpine AS build
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
RUN cat default.conf | grep -n index | head -n 1 | awk -F'[:]' '{print $1}' | xargs -I {} sed -i '{} i try_files $uri /index.html;' default.conf
WORKDIR /
