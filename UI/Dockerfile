### STAGE 1: Build ###
FROM node:13.10-alpine AS build
ENV CI=true
WORKDIR /usr/src/app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run release
### STAGE 2: Run ###
FROM nginx:1.17-alpine
EXPOSE 80 443
COPY --from=build /usr/src/app/dist/AirTeamUI /usr/share/nginx/html
