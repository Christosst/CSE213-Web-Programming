"""Read-only checks after deploying the course website."""
import argparse
import json
import time
import urllib.error
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument('--url', required=True)
parser.add_argument('--swagger-enabled', choices=['true', 'false'], default='true')
args = parser.parse_args()
base = args.url.rstrip('/')

def read(path):
    with urllib.request.urlopen(base + path, timeout=20) as response:
        return response.read().decode('utf-8')

# Allow IIS time to start the newly deployed process; do not mutate classroom data.
for attempt in range(12):
    try:
        hello = json.loads(read('/api/hello'))
        assert hello.get('message') == 'Hello from .NET 10', 'Unexpected hello response'
        assert isinstance(json.loads(read('/api/tasks')), list), 'Tasks did not return an array'
        assert 'Course' in read('/') and 'Demo Library' in read('/'), 'Catalogue is unavailable'
        assert 'Text processing' in read('/text.html'), 'Text frontend is unavailable'
        assert isinstance(json.loads(read('/universityAPI/api/students')), list), 'University students are unavailable'
        assert isinstance(json.loads(read('/universityAPI/api/courses')), list), 'University courses are unavailable'
        if args.swagger_enabled == 'true':
            document = json.loads(read('/openapi/v1.json'))
            assert '/api/text/analysis' in document['paths'], 'Text API is absent from OpenAPI'
            assert 'Swagger' in read('/swagger/index.html'), 'Swagger UI is unavailable'
            university = json.loads(read('/universityAPI/openapi/v1.json'))
            assert '/api/enrollments' in university['paths'], 'University enrollment API is absent'
            assert 'Swagger' in read('/universityAPI/swagger/index.html'), 'University Swagger UI is unavailable'
        print('Verified deployed catalogue, both APIs, frontend and configured Swagger.')
        break
    except (OSError, ValueError, AssertionError) as error:
        if attempt == 11:
            raise SystemExit(f'Deployment verification failed: {error}')
        time.sleep(5)
