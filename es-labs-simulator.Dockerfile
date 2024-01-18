# Use this image as the platform to build the app
FROM node:20-slim AS external-website

# The WORKDIR instruction sets the working directory for everything that will happen next
WORKDIR /app

COPY . .

RUN npm i --force

# Build SvelteKit app
RUN npm run build

# Delete source code files that were used to build the app that are no longer needed
RUN rm -rf src/ static/ emailTemplates/ docker-compose.yml

# The USER instruction sets the user name to use as the default user for the remainder of the current stage
USER node:node

# This is the command that will be run inside the image when you tell Docker to start the container
CMD ["node","build/index.js"]