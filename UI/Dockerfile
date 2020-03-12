### STAGE 1: Build ###
FROM node:13.10-alpine AS build
WORKDIR /usr/src/app
COPY package.json ./
RUN npm install
COPY . .
RUN npm run release
### STAGE 2: Run ###
FROM nginx:1.17-alpine
EXPOSE 80,443
COPY --from=build /usr/src/app/dist/AirTeamUI /usr/share/nginx/html
