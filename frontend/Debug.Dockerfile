# Create image based off of the official 12.8-alpine
FROM node:22-alpine

# Disable Angular CLI analytics
ENV NG_CLI_ANALYTICS=ci

# Set working directory
WORKDIR /app

RUN npm install -g @angular/cli@latest
# Copy dependency definitions
COPY package.json .

## installing and Storing node modules on a separate layer will prevent unnecessary npm installs at each build
RUN npm install

COPY . .

EXPOSE 4200 49153

# Start the application in debug mode
CMD ["npm", "run", "start:debug"]