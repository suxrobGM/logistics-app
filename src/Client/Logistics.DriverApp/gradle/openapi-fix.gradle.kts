/**
 * OpenAPI Generator Post-Processing Fix
 *
 * This script fixes bugs in the OpenAPI-generated Kotlin Multiplatform code.
 * The generator produces invalid code for file uploads that needs to be corrected.
 *
 * Bug: file?.apply { append(File) }  - 'File' is undefined
 * Fix: file?.let { append(it) }
 */

tasks.register("fixOpenApiGenerated") {
    dependsOn("openApiGenerate")
    doLast {
        val generatedDir = layout.buildDirectory.dir("generated/openapi").get().asFile
        if (generatedDir.exists()) {
            fileTree(generatedDir) {
                include("**/*.kt")
            }.forEach { file ->
                var content = file.readText()
                var modified = false

                // Fix 1: Replace buggy file append pattern with null-safe operator
                // Pattern: someVar?.apply { append(File) } -> someVar?.let { append(it) }
                val fileAppendRegex = Regex("""(\w+)\?\.apply\s*\{\s*append\(File\)\s*\}""")
                if (fileAppendRegex.containsMatchIn(content)) {
                    content = fileAppendRegex.replace(content) { matchResult ->
                        "${matchResult.groupValues[1]}?.let { append(it) }"
                    }
                    modified = true
                }

                // Fix 2: Handle cases without the null-safe operator
                // Pattern: someVar.apply { append(File) } -> someVar.let { append(it) }
                val fileAppendRegex2 = Regex("""(\w+)\.apply\s*\{\s*append\(File\)\s*\}""")
                if (fileAppendRegex2.containsMatchIn(content)) {
                    content = fileAppendRegex2.replace(content) { matchResult ->
                        "${matchResult.groupValues[1]}.let { append(it) }"
                    }
                    modified = true
                }

                if (modified) {
                    file.writeText(content)
                    println("Fixed file upload code in: ${file.name}")
                }
            }
        }
    }
}

// Ensure OpenAPI code is generated and fixed before Kotlin compilation
tasks.matching { it.name.startsWith("compile") && it.name.contains("Kotlin") }.configureEach {
    dependsOn("fixOpenApiGenerated")
}
